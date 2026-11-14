using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnglishPractice.Mobile.Models;
using EnglishPractice.Mobile.Views;

namespace EnglishPractice.Mobile.ViewModels;

public partial class HomeViewModel : ObservableObject
{
    public ObservableCollection<Scenario> Scenarios { get; } = new(new[]
    {
        new Scenario { Id = "daily",    Name = "Daily Conversation", Description = "Practice everyday English",          Icon = "☀️",  Difficulty = "Beginner"     },
        new Scenario { Id = "restaurant",Name = "Restaurant",        Description = "Order food, make reservations",      Icon = "🍽️", Difficulty = "Beginner"     },
        new Scenario { Id = "interview", Name = "Job Interview",     Description = "Professional English skills",         Icon = "💼",  Difficulty = "Advanced"     },
        new Scenario { Id = "hotel",     Name = "Hotel Check-in",    Description = "Hotel and accommodation",             Icon = "🏨",  Difficulty = "Intermediate" },
        new Scenario { Id = "shopping",  Name = "Shopping",          Description = "Shop and negotiate prices",           Icon = "🛍️", Difficulty = "Beginner"     },
        new Scenario { Id = "airport",   Name = "Airport",           Description = "Navigate airports and flights",       Icon = "✈️",  Difficulty = "Intermediate" },
        new Scenario { Id = "travel",    Name = "Travel",            Description = "Travel and sightseeing",              Icon = "🗺️", Difficulty = "Intermediate" },
        new Scenario { Id = "business",  Name = "Business Meeting",  Description = "Professional meetings",               Icon = "📊",  Difficulty = "Advanced"     },
    });

    [RelayCommand]
    private async Task StartScenarioAsync(Scenario scenario)
    {
        await Shell.Current.GoToAsync(
            $"{nameof(ConversationPage)}?scenario={Uri.EscapeDataString(scenario.Id)}&scenarioName={Uri.EscapeDataString(scenario.Name)}&level={Uri.EscapeDataString(scenario.Difficulty)}");
    }
}
