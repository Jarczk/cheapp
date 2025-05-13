using MongoDB.Driver;
using Cheapp.Models;

namespace Cheapp.Services;

public interface IConversationService
{
    Task<string> NewSessionAsync(string userId, CancellationToken ct = default);
    Task AppendAsync(string sessionId, string role, string content, CancellationToken ct = default);
    Task<IEnumerable<ChatMessage>> GetMessagesAsync(string sessionId, CancellationToken ct = default);
}

public class ConversationService : IConversationService
{
    private readonly IMongoCollection<ChatSession> _sessions;
    private readonly IMongoCollection<ChatMessage> _messages;
    public ConversationService(IMongoDatabase db)
    {
        _sessions  = db.GetCollection<ChatSession>("chat_sessions");
        _messages  = db.GetCollection<ChatMessage>("chat_messages");
    }

    public async Task<string> NewSessionAsync(string userId, CancellationToken ct = default)
    {
        var s = new ChatSession { UserId = userId };
        await _sessions.InsertOneAsync(s, cancellationToken: ct);
        return s.Id;
    }

    public async Task AppendAsync(string sessionId, string role, string content, CancellationToken ct = default)
    {
        var m = new ChatMessage { SessionId = sessionId, Role = role, Content = content };
        await _messages.InsertOneAsync(m, cancellationToken: ct);
    }

    public async Task<IEnumerable<ChatMessage>> GetMessagesAsync(string sessionId, CancellationToken ct = default)
    {
        var filter = Builders<ChatMessage>.Filter.Eq(x => x.SessionId, sessionId);
        return await _messages.Find(filter).SortBy(x => x.Timestamp).ToListAsync(ct);
    }
}