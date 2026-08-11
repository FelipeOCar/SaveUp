using SaveUp.ViewModels;

namespace SaveUp.Views;

public partial class EntriesPage : ContentPage
{
    private EntriesViewModel ViewModel => (EntriesViewModel)BindingContext;

    public EntriesPage()
    {
        InitializeComponent();
        BindingContext = new EntriesViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.LoadAsync();
    }
}
