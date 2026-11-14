using EnglishPractice.Application.DTOs.Conversation;
namespace EnglishPractice.Application.Interfaces;
public interface IConversationService
{
    Task<ConversationResponse> SendMessageAsync(string userId, SendMessageRequest request, CancellationToken ct = default);
}
