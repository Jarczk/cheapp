namespace Cheapp.Options;
public record GroqOptions
{
    public string BaseUrl { get; init; } = "https://api.groq.com/openai/v1/";
    public string ApiKey { get; init; } = string.Empty;
    public string Model { get; init; } = "mixtral-8x7b-32768";
}