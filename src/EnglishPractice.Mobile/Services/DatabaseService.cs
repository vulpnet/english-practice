using EnglishPractice.Mobile.Models;
using SQLite;

namespace EnglishPractice.Mobile.Services;

public class DatabaseService
{
    private SQLiteAsyncConnection? _db;

    private async Task<SQLiteAsyncConnection> GetDb()
    {
        if (_db != null) return _db;
        var path = Path.Combine(FileSystem.AppDataDirectory, "english_practice.db");
        _db = new SQLiteAsyncConnection(path, SQLiteOpenFlags.ReadWrite | SQLiteOpenFlags.Create | SQLiteOpenFlags.SharedCache);
        await _db.CreateTableAsync<ConversationRecord>();
        await _db.CreateTableAsync<MessageRecord>();
        return _db;
    }

    public async Task<string> CreateConversationAsync(string scenario, string level)
    {
        var db = await GetDb();
        var record = new ConversationRecord
        {
            Id = Guid.NewGuid().ToString(),
            Scenario = scenario,
            EnglishLevel = level,
            Title = $"{scenario} - {DateTime.Now:MMM dd HH:mm}"
        };
        await db.InsertAsync(record);
        return record.Id;
    }

    public async Task SaveMessageAsync(MessageRecord message)
    {
        var db = await GetDb();
        await db.InsertAsync(message);
        await db.ExecuteAsync("UPDATE Conversations SET UpdatedAt=? WHERE Id=?", DateTime.UtcNow, message.ConversationId);
    }

    public async Task<List<ConversationRecord>> GetConversationsAsync()
    {
        var db = await GetDb();
        return await db.Table<ConversationRecord>().OrderByDescending(c => c.UpdatedAt).ToListAsync();
    }

    public async Task<List<MessageRecord>> GetMessagesAsync(string conversationId)
    {
        var db = await GetDb();
        return await db.Table<MessageRecord>().Where(m => m.ConversationId == conversationId).OrderBy(m => m.CreatedAt).ToListAsync();
    }

    public async Task DeleteConversationAsync(string conversationId)
    {
        var db = await GetDb();
        await db.ExecuteAsync("DELETE FROM Messages WHERE ConversationId=?", conversationId);
        await db.ExecuteAsync("DELETE FROM Conversations WHERE Id=?", conversationId);
    }
}
