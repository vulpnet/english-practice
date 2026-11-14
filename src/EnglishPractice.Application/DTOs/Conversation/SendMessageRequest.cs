using System.ComponentModel.DataAnnotations;
namespace EnglishPractice.Application.DTOs.Conversation;
public class SendMessageRequest
{
    [Required][MinLength(1)][MaxLength(2000)]
    public string Message { get; set; } = string.Empty;
    [Required] public string Scenario { get; set; } = string.Empty;
    public string Level { get; set; } = "Beginner";
    public List<ConversationMessageDto> History { get; set; } = new();
}
