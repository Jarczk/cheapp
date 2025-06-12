using Cheapp.Models;
using System.Text.Json;
using System.Text.Json.Serialization;
namespace Cheapp.Services;

public interface IEbayClient
{
    Task<IEnumerable<Offer>> SearchAsync(string query, CancellationToken ct = default);
    Task<Offer?> GetByIdAsync(string itemId, CancellationToken ct = default);
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
        var offers = data?.ItemSummaries?.Select(ToOffer).ToList() ?? new List<Offer>();

        var tasks = offers
            .Where(o => string.IsNullOrEmpty(o.ImageUrl))
            .Select(async o =>
            {
                var full = await GetByIdAsync(o.Id, ct);
                if (full is not null)
                {
                    o.ImageUrl = full.ImageUrl;
                }
            });

        await Task.WhenAll(tasks);
        return offers;
    }

    public async Task<Offer?> GetByIdAsync(string itemId, CancellationToken ct = default)
    {
        var resp = await _http.GetAsync($"item/{itemId}", ct);
        if (!resp.IsSuccessStatusCode) return null;

        var dto = await resp.Content.ReadFromJsonAsync<ItemDto>(_jsonOpt, ct);
        return dto == null ? null : new Offer
        {
            Id         = dto.ItemId,
            Marketplace= "EBAY_US",
            ImageUrl   = dto.Image.ImageUrl,
            Title      = dto.Title,
            Price      = dto.Price.Value,
            Currency   = dto.Price.Currency,
            Url        = dto.ItemWebUrl,
            RetrievedAt= DateTime.UtcNow
        };
    }
    private sealed class ItemDto
    {
        public string ItemId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public PriceObj Price { get; set; } = new();
        public string ItemWebUrl { get; set; } = string.Empty;
        public ImageObj Image { get; set; } = new();
    }

    private static Offer ToOffer(ItemSummary x) => new()
    {
        Id = x.ItemId,
        Marketplace = "EBAY_US",
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
    private sealed class PriceObj
    {
        [JsonPropertyName("value")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public decimal Value { get; set; }

        [JsonPropertyName("currency")]
        public string Currency { get; set; } = "USD";
    }
    private sealed class ImageObj { public int Height { get; set; } public string ImageUrl { get; set; } = ""; public int width { get; set; } }
}