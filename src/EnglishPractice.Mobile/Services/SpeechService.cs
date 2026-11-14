using CommunityToolkit.Maui.Media;

namespace EnglishPractice.Mobile.Services;

public class SpeechService
{
    private CancellationTokenSource? _ttsCts;

    public async Task<string?> RecognizeSpeechAsync(CancellationToken ct = default)
    {
        var status = await Permissions.RequestAsync<Permissions.Microphone>();
        if (status != PermissionStatus.Granted) return null;

        try
        {
            var result = await SpeechToText.Default.ListenAsync(
                new System.Globalization.CultureInfo("en-US"),
                new Progress<string>(),
                ct);
            return result?.IsSuccessful == true ? result.Text : null;
        }
        catch (FeatureNotSupportedException)
        {
            return null;
        }
    }

    public async Task SpeakAsync(string text, CancellationToken ct = default)
    {
        _ttsCts?.Cancel();
        _ttsCts = CancellationTokenSource.CreateLinkedTokenSource(ct);
        try
        {
            var locales = await TextToSpeech.Default.GetLocalesAsync();
            var locale = locales.FirstOrDefault(l => l.Language.StartsWith("en") && l.Country == "US")
                ?? locales.FirstOrDefault(l => l.Language.StartsWith("en"));

            await TextToSpeech.Default.SpeakAsync(text, new SpeechOptions
            {
                Volume = 1.0f,
                Pitch = 1.0f,
                Locale = locale
            }, _ttsCts.Token);
        }
        catch (OperationCanceledException) { }
    }

    public void StopSpeaking() => _ttsCts?.Cancel();
}
