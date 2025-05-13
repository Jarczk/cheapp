using System.Net.Http.Headers;

namespace Cheapp.Services;

public class EbayAuthHandler : DelegatingHandler
{
    private readonly IEbayOAuthService _oauth;
    public EbayAuthHandler(IEbayOAuthService oauth) => _oauth = oauth;

    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request, CancellationToken cancellationToken)
    {
        var token = await _oauth.GetTokenAsync(cancellationToken);
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);
        return await base.SendAsync(request, cancellationToken);
    }
}
