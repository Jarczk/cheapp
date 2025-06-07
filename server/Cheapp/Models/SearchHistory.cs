using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace Cheapp.Models;

public class SearchHistory
{
    [BsonId]
    [BsonRepresentation(BsonType.ObjectId)]
    public string Id { get; set; } = ObjectId.GenerateNewId().ToString();
    
    [BsonElement("userId")]
    public string UserId { get; set; } = string.Empty;
    
    [BsonElement("query")]
    public string Query { get; set; } = string.Empty;
    
    [BsonElement("searchedAt")]
    public DateTime SearchedAt { get; set; } = DateTime.UtcNow;
    
    [BsonElement("resultCount")]
    public int ResultCount { get; set; }
}