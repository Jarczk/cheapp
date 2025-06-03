using Cheapp.Models;

namespace Cheapp.Services;

public interface ISearchHistoryService
{
    Task AddSearchAsync(string userId, string query, int resultCount, CancellationToken ct = default);
    Task<IEnumerable<SearchHistoryResponse>> GetUserHistoryAsync(string userId, int limit = 50, CancellationToken ct = default);
    Task ClearUserHistoryAsync(string userId, CancellationToken ct = default);
    Task DeleteSearchAsync(string userId, string searchId, CancellationToken ct = default);
}
public class SearchHistoryResponse
{
    public string Id { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public DateTime SearchedAt { get; set; }
    public int ResultCount { get; set; }
}