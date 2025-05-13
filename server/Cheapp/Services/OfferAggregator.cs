using Cheapp.Models;
namespace Cheapp.Services;

public interface IOfferAggregator
{
    Task<IEnumerable<Offer>> GetBestAsync(string query, CancellationToken ct = default);
}

public class OfferAggregator : IOfferAggregator
{
    private readonly IEbayClient _ebay;
    public OfferAggregator(IEbayClient ebay) => _ebay = ebay;

    public async Task<IEnumerable<Offer>> GetBestAsync(string query, CancellationToken ct = default)
    {
        var offers = await _ebay.SearchAsync(query, ct);
        return offers.OrderBy(o => o.Price).Take(10);
    }
}