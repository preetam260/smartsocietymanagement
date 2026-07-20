namespace SmartSociety.Repository.Ai;

public class AnthropicOptions
{
    public string ApiKey {get; set;} = string.Empty;
    public string Model {get; set;} = "claude-sonnet-4-6";
    public string BaseUrl {get; set;} = "https://api.anthropic.com/v1/messages";
}