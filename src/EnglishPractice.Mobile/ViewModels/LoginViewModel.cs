using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using EnglishPractice.Mobile.Services;

namespace EnglishPractice.Mobile.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly ApiService _api;

    [ObservableProperty] private string username = string.Empty;
    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string password = string.Empty;
    [ObservableProperty] private bool isRegistering;
    [ObservableProperty] private bool isBusy;
    [ObservableProperty] private string errorMessage = string.Empty;

    public LoginViewModel(ApiService api) => _api = api;

    [RelayCommand]
    private async Task SubmitAsync()
    {
        if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(Password)) return;
        IsBusy = true;
        ErrorMessage = string.Empty;
        try
        {
            bool ok = IsRegistering
                ? await _api.RegisterAsync(Username, Email, Password)
                : await _api.LoginAsync(Username, Password);

            if (ok)
                await Shell.Current.GoToAsync("//HomePage");
            else
                ErrorMessage = IsRegistering ? "Registration failed. Try a different username." : "Invalid username or password.";
        }
        catch (Exception ex) { ErrorMessage = ex.Message; }
        finally { IsBusy = false; }
    }

    [RelayCommand]
    private void ToggleMode()
    {
        IsRegistering = !IsRegistering;
        ErrorMessage = string.Empty;
    }
}
