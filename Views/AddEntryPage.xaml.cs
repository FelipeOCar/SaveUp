using SaveUp.ViewModels;

namespace SaveUp.Views;

public partial class AddEntryPage : ContentPage
{
    public AddEntryPage()
    {
        InitializeComponent();
        BindingContext = new AddEntryViewModel();
    }
}
