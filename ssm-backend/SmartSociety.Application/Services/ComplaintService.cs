using SmartSociety.Application.DTOs;
using SmartSociety.Application.Exceptions;
using SmartSociety.Application.Interfaces;
using SmartSociety.Domain.Enums;
using SmartSociety.Domain.Models;
using SmartSociety.Repository.Interfaces;

namespace SmartSociety.Application.Services;

public class ComplaintService : IComplaintService
{
    private readonly IUnitOfWork _uow;
    private readonly INotificationService _notificationService;

    public ComplaintService(IUnitOfWork uow, INotificationService notificationService)
    {
        _uow = uow;
        _notificationService = notificationService;
    }

    public async Task<ComplaintResponseDto> CreateAsync(CreateComplaintDto dto, Guid userId)
    {
        _ = await _uow.Users.GetByIdAsync(userId)
            ?? throw new NotFoundException("User", userId);

        _ = await _uow.Apartments.GetByIdAsync(dto.ApartmentId)
            ?? throw new NotFoundException("Apartment", dto.ApartmentId);

        var complaint = new Complaint
        {
            UserId = userId,
            ApartmentId = dto.ApartmentId,
            Title = dto.Title,
            Description = dto.Description,
            Status = ComplaintStatus.Open
        };

        await _uow.Complaints.AddAsync(complaint);
        await _uow.SaveChangesAsync();

        return await MapToDtoAsync(complaint);
    }

    public async Task<IEnumerable<ComplaintResponseDto>> GetMyComplaintsAsync(Guid userId)
    {
        var complaints = await _uow.Complaints.GetByUserIdAsync(userId);
        return await MapToDtoListAsync(complaints);
    }

    public async Task<IEnumerable<ComplaintResponseDto>> GetAllAsync()
    {
        var complaints = await _uow.Complaints.GetAllAsync();
        return await MapToDtoListAsync(complaints);
    }

    public async Task<ComplaintResponseDto> ResolveAsync(Guid id, ResolveComplaintDto dto)
    {
        var complaint = await _uow.Complaints.GetByIdAsync(id)
            ?? throw new NotFoundException("Complaint", id);

        if (complaint.Status != ComplaintStatus.Open)
            throw new BadRequestException("Only open complaints can be resolved.");

        complaint.Status = ComplaintStatus.Resolved;
        complaint.AdminResponse = dto.AdminResponse;
        complaint.UpdatedAt = DateTime.UtcNow;

        await _uow.Complaints.UpdateAsync(complaint);
        await _uow.SaveChangesAsync();

        await _notificationService.CreateAsync(
            complaint.UserId,
            "Complaint Resolved",
            $"Your complaint \"{complaint.Title}\" has been resolved. Response: {dto.AdminResponse}");

        return await MapToDtoAsync(complaint);
    }

    public async Task<ComplaintResponseDto> UpdateStatusAsync(Guid id, UpdateComplaintStatusDto dto)
    {
        var complaint = await _uow.Complaints.GetByIdAsync(id)
            ?? throw new NotFoundException("Complaint", id);

        // Validate transition
        var validTransitions = new Dictionary<ComplaintStatus, IEnumerable<ComplaintStatus>>
        {
            [ComplaintStatus.Open] = [ComplaintStatus.InProgress, ComplaintStatus.Resolved, ComplaintStatus.Rejected],
            [ComplaintStatus.InProgress] = [ComplaintStatus.Resolved, ComplaintStatus.Rejected],
        };

        if (!validTransitions.TryGetValue(complaint.Status, out var allowed) || !allowed.Contains(dto.Status))
            throw new BadRequestException(
                $"Cannot transition complaint from '{complaint.Status}' to '{dto.Status}'.");

        if ((dto.Status == ComplaintStatus.Resolved || dto.Status == ComplaintStatus.Rejected)
            && string.IsNullOrWhiteSpace(dto.AdminResponse))
            throw new BadRequestException("A response is required when resolving or rejecting a complaint.");

        complaint.Status = dto.Status;
        if (!string.IsNullOrWhiteSpace(dto.AdminResponse))
            complaint.AdminResponse = dto.AdminResponse;
        complaint.UpdatedAt = DateTime.UtcNow;

        await _uow.Complaints.UpdateAsync(complaint);
        await _uow.SaveChangesAsync();

        var statusLabel = dto.Status.ToString();
        var notifMessage = dto.Status == ComplaintStatus.InProgress
            ? $"Your complaint \"{complaint.Title}\" is now being reviewed."
            : $"Your complaint \"{complaint.Title}\" has been {statusLabel.ToLower()}. Response: {dto.AdminResponse}";

        await _notificationService.CreateAsync(complaint.UserId, $"Complaint {statusLabel}", notifMessage);

        return await MapToDtoAsync(complaint);
    }

    private async Task<ComplaintResponseDto> MapToDtoAsync(Complaint c)
    {
        var user = await _uow.Users.GetByIdAsync(c.UserId);
        var apartment = await _uow.Apartments.GetByIdAsync(c.ApartmentId);
        return new ComplaintResponseDto
        {
            Id = c.Id,
            UserId = c.UserId,
            UserName = user?.Name ?? "",
            ApartmentId = c.ApartmentId,
            ApartmentBlock = apartment?.Block ?? "",
            ApartmentNumber = apartment?.Number ?? "",
            Title = c.Title,
            Description = c.Description,
            Status = c.Status,
            AdminResponse = c.AdminResponse,
            CreatedAt = c.CreatedAt
        };
    }

    private async Task<IEnumerable<ComplaintResponseDto>> MapToDtoListAsync(IEnumerable<Complaint> complaints)
    {
        var result = new List<ComplaintResponseDto>();
        foreach (var c in complaints)
            result.Add(await MapToDtoAsync(c));
        return result;
    }
}
