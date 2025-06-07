using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Cheapp.Models;

public class Favorite
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = string.Empty;
    
    public string UserId { get; set; } = string.Empty;
    
    public string ProductId { get; set; } = string.Empty;
    
    public DateTime AddedAt { get; set; } = DateTime.UtcNow;
    
    [BsonIgnoreIfDefault]
    public string Notes { get; set; } = string.Empty;
}