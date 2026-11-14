using System.Security.Claims;
using EnglishPractice.Application.DTOs;
using EnglishPractice.Application.DTOs.Conversation;
using EnglishPractice.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;

namespace EnglishPractice.API.Controllers;

[ApiController]
[Route("api/conversations")]
[Authorize]
[EnableRateLimiting("api")]
public class ConversationsController : ControllerBase
{
    private readonly IConversationService _conversation;
    public ConversationsController(IConversationService conversation) => _conversation = conversation;

    [HttpPost("{id}/messages")]
    public async Task<IActionResult> SendMessage(string id, [FromBody] SendMessageRequest request, CancellationToken ct)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException();
        var correlationId = HttpContext.Items["CorrelationId"]?.ToString();
        var result = await _conversation.SendMessageAsync(userId, request, ct);
        return Ok(ApiResponse<ConversationResponse>.Ok(result, correlationId));
    }
}
