using SaveUp.ViewModels;

namespace SaveUp.Views;

public partial class EditEntryPage : ContentPage, IQueryAttributable
{
    private Guid? _entryId;

    private EditEntryViewModel ViewModel => (EditEntryViewModel)BindingContext;

    public EditEntryPage()
    {
        InitializeComponent();
        BindingContext = new EditEntryViewModel();
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("EntryId", out object? value) && value is Guid id)
        {
            _entryId = id;
        }
    }

    protected override async void OnAppearing()
    {
        base.OnAppearing();

        if (_entryId.HasValue)
        {
            await ViewModel.LoadAsync(_entryId.Value);
        }
    }
}
