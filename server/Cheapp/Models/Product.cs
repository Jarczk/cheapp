namespace Cheapp.Models;

public class Product
{
    public string Id    { get; set; } = Guid.NewGuid().ToString();
    public string Title { get; set; } = string.Empty;
    public string? Gtin { get; set; }
}