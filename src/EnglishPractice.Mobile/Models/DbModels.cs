using SQLite;
namespace EnglishPractice.Mobile.Models;

[Table("Conversations")]
public class ConversationRecord
{
    [PrimaryKey] public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Scenario { get; set; } = string.Empty;
    public string EnglishLevel { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
}

[Table("Messages")]
public class MessageRecord
{
    [PrimaryKey] public string Id { get; set; } = Guid.NewGuid().ToString();
    public string ConversationId { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string OriginalText { get; set; } = string.Empty;
    public string? CorrectedText { get; set; }
    public string? AIReply { get; set; }
    public string? Explanation { get; set; }
    public string? NaturalExpression { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
