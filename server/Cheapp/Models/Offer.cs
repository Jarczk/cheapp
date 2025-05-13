namespace Cheapp.Models;

public class Offer
{
    public string Id         { get; set; } = Guid.NewGuid().ToString();
    public string ProductId  { get; set; } = string.Empty;
    public string Marketplace{ get; set; } = string.Empty;
    public string Title      { get; set; } = string.Empty;
    public decimal Price     { get; set; }
    public string Currency   { get; set; } = "USD";
    public string Url        { get; set; } = string.Empty;
    public DateTime RetrievedAt { get; set; } = DateTime.UtcNow;
}