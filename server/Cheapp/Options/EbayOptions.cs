namespace Cheapp.Options;
public record EbayOptions
{
    public string BaseUrl { get; init; } = "https://api.sandbox.ebay.com/buy/browse/v1/";
    public string Marketplace { get; init; } = "EBAY_DE";
    public string ClientId { get; init; } = string.Empty;
    public string ClientSecret { get; init; } = string.Empty;
    public string[] Scopes { get; init; } =
        { "https://api.ebay.com/oauth/api_scope" };
}


