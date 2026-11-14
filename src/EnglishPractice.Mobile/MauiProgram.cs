using CommunityToolkit.Maui;
using EnglishPractice.Mobile.Services;
using EnglishPractice.Mobile.ViewModels;
using EnglishPractice.Mobile.Views;
using Microsoft.Extensions.Logging;

namespace EnglishPractice.Mobile;

public static class MauiProgram
{
    public static MauiApp CreateMauiApp()
    {
        var builder = MauiApp.CreateBuilder();
        builder.UseMaui().UseMauiCommunityToolkit();

        builder.Services.AddSingleton<ApiService>();
        builder.Services.AddSingleton<DatabaseService>();
        builder.Services.AddSingleton<SpeechService>();

        builder.Services.AddSingleton<HomeViewModel>();
        builder.Services.AddSingleton<LoginViewModel>();
        builder.Services.AddTransient<ConversationViewModel>();

        builder.Services.AddSingleton<HomePage>();
        builder.Services.AddSingleton<LoginPage>();
        builder.Services.AddTransient<ConversationPage>();

        builder.Logging.AddDebug();

        return builder.Build();
    }
}
