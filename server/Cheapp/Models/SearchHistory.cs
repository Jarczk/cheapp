namespace Cheapp.Models;

public class SearchHistory
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string UserId { get; set; } = string.Empty;
    public string Query { get; set; } = string.Empty;
    public DateTime SearchedAt { get; set; } = DateTime.UtcNow;
    public int ResultCount { get; set; }
}