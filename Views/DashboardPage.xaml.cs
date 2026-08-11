using SaveUp.ViewModels;

namespace SaveUp.Views;

public partial class DashboardPage : ContentPage
{
    private DashboardViewModel ViewModel => (DashboardViewModel)BindingContext;

    public DashboardPage()
    {
        InitializeComponent();
        BindingContext = new DashboardViewModel();
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();
        await ViewModel.LoadAsync();
    }
}
