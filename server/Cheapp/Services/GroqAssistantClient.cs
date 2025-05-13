using Cheapp.Options;
using Microsoft.Extensions.Options;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using System.Text.Json;


namespace Cheapp.Services;



public interface IAssistantClient
{
    Task<string> AskAsync(string prompt, string? systemPrompt = null, CancellationToken ct = default);
}


internal record ChatCompletionRequest(string Model, IEnumerable<Message> Messages, int Max_Tokens = 500);
internal record Message(string Role, string Content);

public class GroqAssistantClient : IAssistantClient
{
    private readonly HttpClient _http;
    private readonly string _model;

    private static readonly JsonSerializerOptions _json = new()
    {
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        PropertyNamingPolicy = JsonNamingPolicy.SnakeCaseLower // .NET 8
    };

    public GroqAssistantClient(HttpClient http, IOptions<GroqOptions> cfg)
    {
        _http = http;
        _model = cfg.Value.Model;
    }

    public async Task<string> AskAsync(
        string prompt,
        string? systemPrompt = "Jesteś pomocnym asystentem sklepowym. Masz pomóc w wyborze produktu.",
        CancellationToken ct = default)
    {
        var messages = new List<ChatMessage>();

        if (!string.IsNullOrWhiteSpace(systemPrompt))
            messages.Add(new ChatMessage("system", systemPrompt));

        messages.Add(new ChatMessage("user", prompt));

        var req = new ChatCompletionRequest
        (
            Model: _model,
            Messages: messages,
            MaxTokens: 500
        );

        using var res = await _http.PostAsJsonAsync(
            "chat/completions", req, _json, ct);

        if (!res.IsSuccessStatusCode)
        {
            var err = await res.Content.ReadAsStringAsync(ct);
            throw new HttpRequestException(
                $"Groq API error {(int)res.StatusCode}: {err}");
        }

        var body = await res.Content.ReadFromJsonAsync<GroqRes>(_json, ct);
        return body?.Choices?.FirstOrDefault()?.Message?.Content
               ?? "(brak odpowiedzi)";
    }

    // ────────── DTOs ──────────
    private record ChatCompletionRequest(
        [property: JsonPropertyName("model")] string Model,
        [property: JsonPropertyName("messages")] IEnumerable<ChatMessage> Messages,
        [property: JsonPropertyName("max_tokens")] int MaxTokens = 500
    );

    private record ChatMessage(
        [property: JsonPropertyName("role")] string Role,
        [property: JsonPropertyName("content")] string Content
    );

    private record GroqRes
    {
        [JsonPropertyName("choices")]
        public List<Choice> Choices { get; init; } = [];

        public record Choice(
            [property: JsonPropertyName("message")] ChatMessage Message
        );
    }
}