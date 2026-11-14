using EnglishPractice.Application.DTOs.Conversation;
using EnglishPractice.Application.Interfaces;
using EnglishPractice.Infrastructure.Services;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace EnglishPractice.UnitTests.Services;

public class ConversationServiceTests
{
    private readonly Mock<IOllamaClient> _ollamaMock = new();
    private readonly ConversationService _sut;

    public ConversationServiceTests()
        => _sut = new ConversationService(_ollamaMock.Object, NullLogger<ConversationService>.Instance);

    [Fact]
    public async Task SendMessageAsync_ValidJsonResponse_ParsesCorrectly()
    {
        var json = """{"reply":"Hi!","originalText":"hello","correctedText":"Hello!","explanation":"","naturalExpression":""}""";
        _ollamaMock.Setup(x => x.ChatAsync(It.IsAny<string>(), It.IsAny<List<(string, string)>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(json);

        var result = await _sut.SendMessageAsync("user1", new SendMessageRequest { Message = "hello", Scenario = "Daily" });

        Assert.Equal("Hi!", result.Reply);
        Assert.Equal("Hello!", result.CorrectedText);
    }

    [Fact]
    public async Task SendMessageAsync_InvalidJson_FallsBackToRaw()
    {
        _ollamaMock.Setup(x => x.ChatAsync(It.IsAny<string>(), It.IsAny<List<(string, string)>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync("Just a plain text response.");

        var result = await _sut.SendMessageAsync("user1", new SendMessageRequest { Message = "hi", Scenario = "Daily" });

        Assert.Equal("Just a plain text response.", result.Reply);
    }

    [Fact]
    public async Task SendMessageAsync_OllamaThrows_Rethrows()
    {
        _ollamaMock.Setup(x => x.ChatAsync(It.IsAny<string>(), It.IsAny<List<(string, string)>>(), It.IsAny<CancellationToken>()))
            .ThrowsAsync(new HttpRequestException("Connection refused"));

        await Assert.ThrowsAsync<HttpRequestException>(() =>
            _sut.SendMessageAsync("user1", new SendMessageRequest { Message = "hi", Scenario = "Daily" }));
    }

    [Fact]
    public async Task SendMessageAsync_HistoryTrimmedToMax20()
    {
        List<(string, string)>? capturedMessages = null;
        _ollamaMock.Setup(x => x.ChatAsync(It.IsAny<string>(), It.IsAny<List<(string, string)>>(), It.IsAny<CancellationToken>()))
            .Callback<string, List<(string, string)>, CancellationToken>((_, msgs, _) => capturedMessages = msgs)
            .ReturnsAsync("""{"reply":"ok","originalText":"test","correctedText":"test","explanation":"","naturalExpression":""}""");

        var history = Enumerable.Range(0, 30).Select(i => new ConversationMessageDto { Role = "user", Content = $"msg{i}" }).ToList();
        await _sut.SendMessageAsync("user1", new SendMessageRequest { Message = "hi", Scenario = "Daily", History = history });

        // system + 20 history + 1 current = 22
        Assert.NotNull(capturedMessages);
        Assert.True(capturedMessages!.Count <= 21);
    }
}
