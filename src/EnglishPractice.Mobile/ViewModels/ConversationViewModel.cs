using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnglishPractice.Mobile.Models;
using EnglishPractice.Mobile.Services;

namespace EnglishPractice.Mobile.ViewModels;

[QueryProperty(nameof(ScenarioId), "scenario")]
[QueryProperty(nameof(ScenarioName), "scenarioName")]
[QueryProperty(nameof(Level), "level")]
public partial class ConversationViewModel : ObservableObject
{
    private readonly ApiService _api;
    private readonly DatabaseService _db;
    private readonly SpeechService _speech;
    private string _conversationId = string.Empty;

    [ObservableProperty] private string scenarioId = string.Empty;
    [ObservableProperty] private string scenarioName = string.Empty;
    [ObservableProperty] private string level = "Beginner";
    [ObservableProperty] private string status = "Tap and hold the mic to speak";
    [ObservableProperty] private bool isRecording;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string currentTranscript = string.Empty;

    public ObservableCollection<ChatMessage> Messages { get; } = new();

    private CancellationTokenSource? _recordingCts;

    public ConversationViewModel(ApiService api, DatabaseService db, SpeechService speech)
    {
        _api = api;
        _db = db;
        _speech = speech;
    }

    public async Task InitializeAsync()
    {
        if (!string.IsNullOrEmpty(_conversationId)) return;
        _conversationId = await _db.CreateConversationAsync(ScenarioName, Level);
        Messages.Add(new ChatMessage
        {
            Role = "assistant",
            Content = $"Hello! Let's practice {ScenarioName}. I'm here to help you improve your English. Go ahead and say something!",
            IsUser = false
        });
    }

    [RelayCommand]
    private async Task StartRecordingAsync()
    {
        if (IsRecording || IsBusy) return;
        IsRecording = true;
        Status = "Listening...";
        CurrentTranscript = string.Empty;
        _recordingCts = new CancellationTokenSource();

        try
        {
            var text = await _speech.RecognizeSpeechAsync(_recordingCts.Token);
            IsRecording = false;

            if (string.IsNullOrWhiteSpace(text))
            {
                Status = "Could not understand. Try again.";
                return;
            }

            CurrentTranscript = text;
            await ProcessMessageAsync(text);
        }
        catch (OperationCanceledException)
        {
            IsRecording = false;
            Status = "Tap and hold the mic to speak";
        }
        catch (Exception ex)
        {
            IsRecording = false;
            Status = $"Error: {ex.Message}";
        }
    }

    [RelayCommand]
    private void StopRecording() => _recordingCts?.Cancel();

    private async Task ProcessMessageAsync(string text)
    {
        IsBusy = true;
        Status = "AI is thinking...";

        Messages.Add(new ChatMessage { Role = "user", Content = text, IsUser = true });

        var history = Messages
            .Select(m => new HistoryItem { Role = m.IsUser ? "user" : "assistant", Content = m.AiReply ?? m.Content })
            .ToList();

        try
        {
            var response = await _api.SendMessageAsync(_conversationId, text, ScenarioName, Level, history);
            if (response == null) throw new InvalidOperationException("No response from server");

            Messages.Add(new ChatMessage
            {
                Role = "assistant",
                Content = response.Reply,
                OriginalText = response.OriginalText,
                CorrectedText = response.CorrectedText,
                Explanation = response.Explanation,
                NaturalExpression = response.NaturalExpression,
                AiReply = response.Reply,
                IsUser = false,
                HasCorrection = response.OriginalText != response.CorrectedText && !string.IsNullOrEmpty(response.CorrectedText)
            });

            await _db.SaveMessageAsync(new MessageRecord
            {
                ConversationId = _conversationId,
                Role = "user",
                OriginalText = text,
                CorrectedText = response.CorrectedText,
                AIReply = response.Reply,
                Explanation = response.Explanation,
                NaturalExpression = response.NaturalExpression
            });

            Status = "Tap and hold the mic to speak";
            await _speech.SpeakAsync(response.Reply);
        }
        catch (Exception ex)
        {
            Status = $"Error: {ex.Message}";
            Messages.Add(new ChatMessage { Role = "assistant", Content = "Sorry, I couldn't process that. Please try again.", IsUser = false });
        }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private async Task SpeakMessageAsync(string text) => await _speech.SpeakAsync(text);

    [RelayCommand]
    private void StopSpeaking() => _speech.StopSpeaking();
}

public class ChatMessage
{
    public string Role { get; set; } = string.Empty;
    public string Content { get; set; } = string.Empty;
    public string? AiReply { get; set; }
    public string? OriginalText { get; set; }
    public string? CorrectedText { get; set; }
    public string? Explanation { get; set; }
    public string? NaturalExpression { get; set; }
    public bool IsUser { get; set; }
    public bool HasCorrection { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.Now;
}
