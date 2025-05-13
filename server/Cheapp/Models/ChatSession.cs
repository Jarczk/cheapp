namespace Cheapp.Models;

public class ChatSession
{
    public string Id      { get; set; } = Guid.NewGuid().ToString();
    public string UserId  { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}