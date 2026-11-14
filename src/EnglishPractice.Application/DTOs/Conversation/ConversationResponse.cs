namespace EnglishPractice.Application.DTOs.Conversation;
public class ConversationResponse
{
    public string Reply { get; set; } = string.Empty;
    public string OriginalText { get; set; } = string.Empty;
    public string CorrectedText { get; set; } = string.Empty;
    public string Explanation { get; set; } = string.Empty;
    public string NaturalExpression { get; set; } = string.Empty;
}
