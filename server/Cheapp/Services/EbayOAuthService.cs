using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;
using Cheapp.Options;
using Microsoft.Extensions.Options;

namespace Cheapp.Services;

public interface IEbayOAuthService
{
    Task<string> GetTokenAsync(CancellationToken ct = default);
}

public class EbayOAuthService : IEbayOAuthService
{
    private readonly HttpClient _http;
    private readonly EbayOptions _opt;
    private readonly SemaphoreSlim _lock = new(1, 1);

    private string? _accessToken;
    private DateTime _expiresUtc;

    public EbayOAuthService(IHttpClientFactory factory, IOptions<EbayOptions> opt)
    {
        _http = factory.CreateClient("EbayAuth");
        _opt = opt.Value;
    }

    public async Task<string> GetTokenAsync(CancellationToken ct = default)
    {
        if (_accessToken is not null && DateTime.UtcNow < _expiresUtc.AddMinutes(-5))
            return _accessToken;

        await _lock.WaitAsync(ct);
        try
        {
            if (_accessToken is not null && DateTime.UtcNow < _expiresUtc.AddMinutes(-5))
                return _accessToken;

            var body = new Dictionary<string, string>
            {
                ["grant_type"] = "client_credentials",
                ["scope"] = string.Join(' ', _opt.Scopes)
            };

            var req = new HttpRequestMessage(HttpMethod.Post, "identity/v1/oauth2/token")
            {
                Content = new FormUrlEncodedContent(body)
            };

            var basic = Convert.ToBase64String(
                Encoding.ASCII.GetBytes($"{_opt.ClientId}:{_opt.ClientSecret}"));
            req.Headers.Authorization = new AuthenticationHeaderValue("Basic", basic);

            var res = await _http.SendAsync(req, ct);
            res.EnsureSuccessStatusCode();

            var tok = await res.Content.ReadFromJsonAsync<TokenRes>(cancellationToken: ct);
            _accessToken = tok!.AccessToken;
            _expiresUtc = DateTime.UtcNow.AddSeconds(tok.ExpiresIn);
            return _accessToken;
        }
        finally { _lock.Release(); }
    }

    private sealed record TokenRes(
        [property: JsonPropertyName("access_token")] string AccessToken,
        [property: JsonPropertyName("expires_in")] int ExpiresIn);
}
