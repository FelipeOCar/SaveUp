using System.Globalization;
using System.Windows.Input;
using SaveUp.Models;
using SaveUp.Services;

namespace SaveUp.ViewModels;

/// <summary>
/// Validiert die Formulareingaben und speichert einen neuen Kaufverzicht.
/// </summary>
public sealed class AddEntryViewModel : BaseViewModel
{
    private readonly ISavingEntryService _savingEntryService;
    private readonly Command _saveCommand;
    private string _description = string.Empty;
    private string _amountText = string.Empty;
    private DateTime _date = DateTime.Today;
    private TimeSpan _time = DateTime.Now.TimeOfDay;
    private string _descriptionError = string.Empty;
    private string _amountError = string.Empty;
    private string _operationError = string.Empty;
    private bool _isBusy;

    public AddEntryViewModel()
        : this(AppServices.SavingEntries)
    {
    }

    internal AddEntryViewModel(ISavingEntryService savingEntryService)
    {
        _savingEntryService = savingEntryService;
        _saveCommand = new Command(async () => await SaveAsync(), () => !IsBusy);
        SaveCommand = _saveCommand;
        ResetCommand = new Command(ResetForm);
    }

    public string Description
    {
        get => _description;
        set
        {
            value ??= string.Empty;

            if (SetProperty(ref _description, value))
            {
                DescriptionError = string.Empty;
                OperationError = string.Empty;
            }
        }
    }

    public string AmountText
    {
        get => _amountText;
        set
        {
            value ??= string.Empty;

            if (SetProperty(ref _amountText, value))
            {
                AmountError = string.Empty;
                OperationError = string.Empty;
            }
        }
    }

    public DateTime Date
    {
        get => _date;
        set => SetProperty(ref _date, value);
    }

    public TimeSpan Time
    {
        get => _time;
        set => SetProperty(ref _time, value);
    }

    public string DescriptionError
    {
        get => _descriptionError;
        private set
        {
            if (SetProperty(ref _descriptionError, value))
            {
                OnPropertyChanged(nameof(HasDescriptionError));
            }
        }
    }

    public string AmountError
    {
        get => _amountError;
        private set
        {
            if (SetProperty(ref _amountError, value))
            {
                OnPropertyChanged(nameof(HasAmountError));
            }
        }
    }

    public string OperationError
    {
        get => _operationError;
        private set
        {
            if (SetProperty(ref _operationError, value))
            {
                OnPropertyChanged(nameof(HasOperationError));
            }
        }
    }

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                _saveCommand.ChangeCanExecute();
            }
        }
    }

    public bool HasDescriptionError => !string.IsNullOrWhiteSpace(DescriptionError);

    public bool HasAmountError => !string.IsNullOrWhiteSpace(AmountError);

    public bool HasOperationError => !string.IsNullOrWhiteSpace(OperationError);

    public ICommand SaveCommand { get; }

    public ICommand ResetCommand { get; }

    private async Task SaveAsync()
    {
        OperationError = string.Empty;

        if (!ValidateInput(out decimal amount))
        {
            return;
        }

        IsBusy = true;

        try
        {
            SavingEntry entry = new()
            {
                Description = Description.Trim(),
                Amount = amount,
                OccurredAt = Date.Date.Add(Time)
            };

            await _savingEntryService.AddAsync(entry);
            ResetForm();
            await Shell.Current.GoToAsync("//Entries");
        }
        catch (Exception)
        {
            OperationError = "Der Eintrag konnte nicht gespeichert werden. Bitte versuche es erneut.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private bool ValidateInput(out decimal amount)
    {
        DescriptionError = string.IsNullOrWhiteSpace(Description)
            ? "Bitte gib eine Kurzbeschreibung ein."
            : string.Empty;

        AmountError = TryParseAmount(AmountText, out amount) && amount > 0
            ? string.Empty
            : "Der Betrag muss grösser als CHF 0.00 sein.";

        return !HasDescriptionError && !HasAmountError;
    }

    private static bool TryParseAmount(string value, out decimal amount)
    {
        amount = 0;

        if (string.IsNullOrWhiteSpace(value))
        {
            return false;
        }

        string normalizedValue = value
            .Replace("CHF", string.Empty, StringComparison.OrdinalIgnoreCase)
            .Replace("'", string.Empty)
            .Trim();

        return decimal.TryParse(normalizedValue, NumberStyles.Number, CultureInfo.CurrentCulture, out amount)
            || decimal.TryParse(normalizedValue.Replace(',', '.'), NumberStyles.Number, CultureInfo.InvariantCulture, out amount);
    }

    private void ResetForm()
    {
        Description = string.Empty;
        AmountText = string.Empty;
        Date = DateTime.Today;
        Time = DateTime.Now.TimeOfDay;
        DescriptionError = string.Empty;
        AmountError = string.Empty;
        OperationError = string.Empty;
    }
}
