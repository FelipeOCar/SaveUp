using System.Collections.ObjectModel;
using System.Windows.Input;
using SaveUp.Models;
using SaveUp.Services;

namespace SaveUp.ViewModels;

/// <summary>
/// Lädt alle Einträge und stellt Aktionen zum Bearbeiten und Löschen bereit.
/// </summary>
public sealed class EntriesViewModel : BaseViewModel
{
    private readonly ISavingEntryService _savingEntryService;
    private readonly IUserDialogService _dialogService;
    private readonly Command _clearAllCommand;
    private readonly Command<SavingEntry> _entryActionsCommand;
    private string _operationError = string.Empty;
    private bool _isBusy;

    public EntriesViewModel()
        : this(AppServices.SavingEntries, AppServices.Dialogs)
    {
    }

    internal EntriesViewModel(
        ISavingEntryService savingEntryService,
        IUserDialogService dialogService)
    {
        _savingEntryService = savingEntryService;
        _dialogService = dialogService;
        NewEntryCommand = new Command(async () => await Shell.Current.GoToAsync("//AddEntry"));
        _clearAllCommand = new Command(async () => await ClearAllAsync(), () => HasEntries && !IsBusy);
        _entryActionsCommand = new Command<SavingEntry>(async entry => await ShowEntryActionsAsync(entry), entry => entry is not null && !IsBusy);
        ClearAllCommand = _clearAllCommand;
        EntryActionsCommand = _entryActionsCommand;

        Entries.CollectionChanged += (_, _) =>
        {
            OnPropertyChanged(nameof(TotalSavings));
            OnPropertyChanged(nameof(HasEntries));
            _clearAllCommand.ChangeCanExecute();
        };
    }

    public ObservableCollection<SavingEntry> Entries { get; } = [];

    public decimal TotalSavings => Entries.Sum(entry => entry.Amount);

    public bool HasEntries => Entries.Count > 0;

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

    public bool HasOperationError => !string.IsNullOrWhiteSpace(OperationError);

    public bool IsBusy
    {
        get => _isBusy;
        private set
        {
            if (SetProperty(ref _isBusy, value))
            {
                _clearAllCommand.ChangeCanExecute();
                _entryActionsCommand.ChangeCanExecute();
            }
        }
    }

    public ICommand ClearAllCommand { get; }

    public ICommand EntryActionsCommand { get; }

    public ICommand NewEntryCommand { get; }

    public async Task LoadAsync()
    {
        IsBusy = true;
        OperationError = string.Empty;

        try
        {
            IReadOnlyList<SavingEntry> entries = await _savingEntryService.GetAllAsync();
            Entries.Clear();

            foreach (SavingEntry entry in entries)
            {
                Entries.Add(entry);
            }
        }
        catch (Exception)
        {
            OperationError = "Die gespeicherten Einträge konnten nicht geladen werden.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ClearAllAsync()
    {
        bool confirmed = await _dialogService.ConfirmAsync(
            "Alle Einträge löschen?",
            $"Möchtest du wirklich alle {Entries.Count} Einträge löschen? Diese Aktion kann nicht rückgängig gemacht werden.",
            "Löschen",
            "Abbrechen");

        if (!confirmed)
        {
            return;
        }

        IsBusy = true;
        OperationError = string.Empty;

        try
        {
            await _savingEntryService.ClearAsync();
            Entries.Clear();
        }
        catch (Exception)
        {
            OperationError = "Die Einträge konnten nicht gelöscht werden.";
        }
        finally
        {
            IsBusy = false;
        }
    }

    private async Task ShowEntryActionsAsync(SavingEntry entry)
    {
        string? selectedAction = await _dialogService.ChooseActionAsync(
            entry.Description,
            "Abbrechen",
            "Bearbeiten",
            "Löschen");

        if (selectedAction == "Bearbeiten")
        {
            await Shell.Current.GoToAsync(
                nameof(SaveUp.Views.EditEntryPage),
                new Dictionary<string, object> { ["EntryId"] = entry.Id });
            return;
        }

        if (selectedAction != "Löschen")
        {
            return;
        }

        bool confirmed = await _dialogService.ConfirmAsync(
            "Eintrag löschen?",
            $"Möchtest du «{entry.Description}» wirklich löschen? Diese Aktion kann nicht rückgängig gemacht werden.",
            "Löschen",
            "Abbrechen");

        if (!confirmed)
        {
            return;
        }

        IsBusy = true;
        OperationError = string.Empty;

        try
        {
            await _savingEntryService.DeleteAsync(entry.Id);
            Entries.Remove(entry);
        }
        catch (Exception)
        {
            OperationError = "Der Eintrag konnte nicht gelöscht werden.";
        }
        finally
        {
            IsBusy = false;
        }
    }
}
