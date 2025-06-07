using Cheapp.Models;
using System.Text.Json;
namespace Cheapp.Services;

public interface IEbayClient
{
    Task<IEnumerable<Offer>> SearchAsync(string query, CancellationToken ct = default);
}

public class EbayClient : IEbayClient
{
    private readonly HttpClient _http;
    public EbayClient(HttpClient http) => _http = http;

    private static readonly JsonSerializerOptions _jsonOpt = new()
    {
        PropertyNameCaseInsensitive = true
    };

    public async Task<IEnumerable<Offer>> SearchAsync(string query, CancellationToken ct = default)
    {
        const string endpoint = "item_summary/search";
        var resp = await _http.GetAsync($"{endpoint}?q={Uri.EscapeDataString(query)}&limit=25", ct);
        resp.EnsureSuccessStatusCode();
        var data = await resp.Content.ReadFromJsonAsync<EbayResp>(cancellationToken: ct);
        return data?.ItemSummaries?.Select(ToOffer) ?? Enumerable.Empty<Offer>();
    }

    private static Offer ToOffer(ItemSummary x) => new()
    {
        Id = x.ItemId,
        Marketplace = "EBAY_DE",
        ImageUrl = x.Image.ImageUrl,
        Title = x.Title,
        Price = x.Price.Value,
        Currency = x.Price.Currency,
        Url = x.ItemWebUrl,
        RetrievedAt = DateTime.UtcNow
    };

    private sealed class EbayResp { public List<ItemSummary>? ItemSummaries { get; set; } }
    private sealed class ItemSummary
    {
        public string ItemId { get; set; } = string.Empty;
        public string Title  { get; set; } = string.Empty;
        public PriceObj Price{ get; set; } = new();
        public string ItemWebUrl { get; set; } = string.Empty;
        public ImageObj Image { get; set; } = new();
    }
    private sealed class PriceObj { public decimal Value { get; set; } public string Currency { get; set; } = "USD"; }
    private sealed class ImageObj { public int Height { get; set; } public string ImageUrl { get; set; } = ""; public int width { get; set; } }
}