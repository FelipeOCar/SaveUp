using System.Globalization;
using System.Windows.Input;
using SaveUp.Models;
using SaveUp.Services;

namespace SaveUp.ViewModels;

/// <summary>
/// Lädt einen bestehenden Kaufverzicht und speichert geänderte Werte wieder in der JSON-Datei.
/// </summary>
public sealed class EditEntryViewModel : BaseViewModel
{
    private readonly ISavingEntryService _savingEntryService;
    private readonly Command _saveCommand;
    private Guid? _entryId;
    private string _description = string.Empty;
    private string _amountText = string.Empty;
    private DateTime _date = DateTime.Today;
    private TimeSpan _time = DateTime.Now.TimeOfDay;
    private string _descriptionError = string.Empty;
    private string _amountError = string.Empty;
    private string _operationError = string.Empty;
    private bool _isBusy;

    public EditEntryViewModel()
        : this(AppServices.SavingEntries)
    {
    }

    internal EditEntryViewModel(ISavingEntryService savingEntryService)
    {
        _savingEntryService = savingEntryService;
        _saveCommand = new Command(async () => await SaveAsync(), () => !IsBusy && _entryId.HasValue);
        SaveCommand = _saveCommand;
        CancelCommand = new Command(async () => await Shell.Current.GoToAsync(".."));
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

    public bool HasDescriptionError => !string.IsNullOrWhiteSpace(DescriptionError);

    public bool HasAmountError => !string.IsNullOrWhiteSpace(AmountError);

    public bool HasOperationError => !string.IsNullOrWhiteSpace(OperationError);

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

    public ICommand SaveCommand { get; }

    public ICommand CancelCommand { get; }

    public async Task LoadAsync(Guid id)
    {
        if (_entryId == id)
        {
            return;
        }

        IsBusy = true;
        OperationError = string.Empty;

        try
        {
            SavingEntry? entry = await _savingEntryService.GetByIdAsync(id);

            if (entry is null)
            {
                OperationError = "Der Eintrag wurde nicht gefunden.";
                return;
            }

            _entryId = entry.Id;
            Description = entry.Description;
            AmountText = entry.Amount.ToString("0.00", CultureInfo.CurrentCulture);
            Date = entry.OccurredAt.Date;
            Time = entry.OccurredAt.TimeOfDay;
        }
        catch (Exception)
        {
            OperationError = "Der Eintrag konnte nicht geladen werden.";
        }
        finally
        {
            IsBusy = false;
            _saveCommand.ChangeCanExecute();
        }
    }

    private async Task SaveAsync()
    {
        OperationError = string.Empty;

        if (!_entryId.HasValue || !ValidateInput(out decimal amount))
        {
            return;
        }

        IsBusy = true;

        try
        {
            SavingEntry entry = new()
            {
                Id = _entryId.Value,
                Description = Description.Trim(),
                Amount = amount,
                OccurredAt = Date.Date.Add(Time)
            };

            await _savingEntryService.UpdateAsync(entry);
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception)
        {
            OperationError = "Die Änderungen konnten nicht gespeichert werden.";
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
}
