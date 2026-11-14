using EnglishPractice.Mobile.ViewModels;

namespace EnglishPractice.Mobile.Views;

public partial class ConversationPage : ContentPage
{
    private readonly ConversationViewModel _vm;

    public ConversationPage(ConversationViewModel vm)
    {
        InitializeComponent();
        _vm = vm;
        BindingContext = vm;
    }

    protected override async void OnNavigatedTo(NavigatedToEventArgs args)
    {
        base.OnNavigatedTo(args);
        await _vm.InitializeAsync();
    }

    protected override void OnDisappearing()
    {
        base.OnDisappearing();
        _vm.StopSpeakingCommand.Execute(null);
    }
}
