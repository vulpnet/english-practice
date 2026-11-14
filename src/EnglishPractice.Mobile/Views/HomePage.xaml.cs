using EnglishPractice.Mobile.ViewModels;

namespace EnglishPractice.Mobile.Views;

public partial class HomePage : ContentPage
{
    public HomePage(HomeViewModel vm)
    {
        InitializeComponent();
        BindingContext = vm;
    }
}
