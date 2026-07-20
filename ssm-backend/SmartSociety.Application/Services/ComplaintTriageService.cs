using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using SmartSociety.Application.Complaints.Triage;
using SmartSociety.Application.DTOs;
using SmartSociety.Domain;
using SmartSociety.Domain.Enums;
using SmartSociety.Domain.Models;
using SmartSociety.Repository.Ai;

namespace SmartSociety.Infrastructure.Ai;

public class ComplaintTriageService : IComplaintTriageService
{
    private readonly HttpClient _http;
    private readonly AnthropicOptions _options;
    private readonly ILogger<ComplaintTriageService> _logger;

    public ComplaintTriageService(
        HttpClient http,
        IOptions<AnthropicOptions> options,
        ILogger<ComplaintTriageService> logger)
    {
        _http = http;
        _options = options.Value;
        _logger = logger;
    }

    public async Task<ComplaintTriageResult?> TriageAsync(
        Complaint complaint,
        IReadOnlyList<Complaint> openComplaintsSameBlock,
        CancellationToken ct = default)
    {
        var prompt = BuildPrompt(complaint, openComplaintsSameBlock);

        var requestBody = new
        {
            model = _options.Model,
            max_tokens = 600,
            system = SystemPrompt,
            messages = new[]
            {
                new { role = "user", content = prompt }
            }
        };

        using var request = new HttpRequestMessage(HttpMethod.Post, _options.BaseUrl)
                            {
                                Content = new StringContent(
                                    JsonSerializer.Serialize(requestBody),
                                    Encoding.UTF8,
                                    "application/json")
                            };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", _options.ApiKey);
        request.Headers.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        string? raw = null;
        try
        {
            using var response = await _http.SendAsync(request, ct);
            raw = await response.Content.ReadAsStringAsync(ct);

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogError(
                    "Anthropic API returned {StatusCode} for complaint {ComplaintId}. Body: {Body}",
                    (int)response.StatusCode,
                    complaint.Id,
                    raw);
                return null;
            }

            using var doc = JsonDocument.Parse(raw);

            var text = doc.RootElement
                .GetProperty("content")[0]
                .GetProperty("text")
                .GetString();

            if (string.IsNullOrWhiteSpace(text))
            {
                _logger.LogWarning("Empty triage response for complaint {ComplaintId}", complaint.Id);
                return null;
            }

            return ParseModelJson(text, complaint.Id);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex,
                "Complaint triage failed for {ComplaintId}. Raw response: {Body}",
                complaint.Id,
                raw ?? "(no response received)");
            return null;
        }
    }

    private static string BuildPrompt(Complaint complaint, IReadOnlyList<Complaint> others)
    {
        var sb = new StringBuilder();
        sb.AppendLine("New complaint to triage:");
        sb.AppendLine($"Id: {complaint.Id}");
        sb.AppendLine($"Title: {complaint.Title}");
        sb.AppendLine($"Description: {complaint.Description}");
        sb.AppendLine();
        sb.AppendLine("Other OPEN complaints currently in the same apartment/block (for duplicate detection):");

        if (others.Count == 0)
        {
            sb.AppendLine("(none)");
        }
        else
        {
            foreach (var o in others)
            {
                sb.AppendLine($"- Id: {o.Id} | Title: {o.Title} | Description: {Truncate(o.Description, 200)} | CreatedAt: {o.CreatedAt:o}");
            }
        }

        return sb.ToString();
    }

    private static string Truncate(string? s, int max) =>
        string.IsNullOrEmpty(s) ? string.Empty : (s.Length <= max ? s : s[..max] + "...");

    private const string SystemPrompt = """
        You are a triage assistant for a residential society complaint system.
        Given a new complaint and a list of other currently open complaints in the same block,
        respond with ONLY a JSON object, no prose, no markdown fences, in exactly this shape:

        {
          "category": "Plumbing|Electrical|Security|Parking|Noise|CommonArea|Other",
          "priority": "Low|Medium|High|Urgent",
          "draftResponse": "A short, polite, actionable draft reply an admin could send to the resident.",
          "possibleDuplicateIds": ["<guid>", "..."]
        }

        Rules:
        - possibleDuplicateIds should only include ids from the provided "other open complaints" list,
          and only when they clearly describe the same underlying issue (e.g. multiple "no water" reports).
        - Urgent = safety/security risk or complete utility outage affecting multiple units.
        - High = utility outage or security concern affecting one unit.
        - Medium = functional issue, not urgent.
        - Low = cosmetic or minor inconvenience.
        - draftResponse should acknowledge the issue and state a plausible next step (e.g. "maintenance has been notified and will inspect within 24 hours"), without inventing specific names, dates, or ticket numbers.
        """;

    private ComplaintTriageResult? ParseModelJson(string text, Guid complaintId)
    {
        var jsonStart = text.IndexOf('{');
        var jsonEnd = text.LastIndexOf('}');
        if (jsonStart < 0 || jsonEnd < jsonStart)
        {
            _logger.LogWarning("Triage response for {ComplaintId} did not contain JSON: {Text}", complaintId, text);
            return null;
        }

        var json = text[jsonStart..(jsonEnd + 1)];
        using var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var category = Enum.TryParse<ComplaintCategory>(root.GetProperty("category").GetString(), true, out var cat)
            ? cat : ComplaintCategory.Other;

        var priority = Enum.TryParse<ComplaintPriority>(root.GetProperty("priority").GetString(), true, out var pri)
            ? pri : ComplaintPriority.Medium;

        var draft = root.TryGetProperty("draftResponse", out var d) ? d.GetString() ?? string.Empty : string.Empty;

        var dupes = new List<Guid>();
        if (root.TryGetProperty("possibleDuplicateIds", out var arr) && arr.ValueKind == JsonValueKind.Array)
        {
            foreach (var item in arr.EnumerateArray())
            {
                if (Guid.TryParse(item.GetString(), out var id))
                    dupes.Add(id);
            }
        }

        return new ComplaintTriageResult
        {
            Category = category,
            Priority = priority,
            DraftResponse = draft,
            PossibleDuplicateIds = dupes
        };
    }
}