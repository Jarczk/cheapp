using Cheapp.Models;
using Cheapp.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cheapp.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AssistantController : ControllerBase
{
    private readonly IAssistantClient _ai;
    private readonly IConversationService _conv;
    public AssistantController(IAssistantClient ai, IConversationService conv)
    {
        _ai = ai;
        _conv = conv;
    }

    //[Authorize]
    [HttpPost("ask")]
    public async Task<ActionResult> Ask([FromBody] AskDto dto, CancellationToken ct)
    {
        var uid = User.FindFirstValue(ClaimTypes.NameIdentifier)!;
        var sessionId = string.IsNullOrEmpty(dto.SessionId) ? await _conv.NewSessionAsync(uid, ct) : dto.SessionId!;
        await _conv.AppendAsync(sessionId, "user", dto.Prompt, ct);

        var answer = await _ai.AskAsync(dto.Prompt, dto.SystemPrompt, ct);
        await _conv.AppendAsync(sessionId, "assistant", answer, ct);

        return Ok(new { sessionId, answer });
    }

    //[Authorize]
    [HttpGet("session/{sessionId}")]
    public async Task<ActionResult<IEnumerable<ChatMessage>>> History(string sessionId, CancellationToken ct)
        => Ok(await _conv.GetMessagesAsync(sessionId, ct));

    public record AskDto(string Prompt, string? SystemPrompt, string? SessionId);
}