using EnglishPractice.Mobile.Services;

namespace EnglishPractice.Mobile;

public partial class App : Application
{
    public App(ApiService api)
    {
        InitializeComponent();
        MainPage = new AppShell();
        // Navigate after shell is set
        Dispatcher.Dispatch(() =>
        {
            if (api.IsLoggedIn)
                Shell.Current.GoToAsync("//HomePage");
            else
                Shell.Current.GoToAsync("//LoginPage");
        });
    }
}
