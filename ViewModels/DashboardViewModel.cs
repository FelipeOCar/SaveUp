using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Input;
using SaveUp.Models;
using SaveUp.Services;

namespace SaveUp.ViewModels;

/// <summary>
/// Berechnet Total, Wochenwert und die Tageswerte für das Dashboard.
/// </summary>
public sealed class DashboardViewModel : BaseViewModel
{
    private readonly ISavingEntryService _savingEntryService;
    private decimal _totalSavings;
    private decimal _weeklySavings;
    private int _entryCount;
    private string _operationError = string.Empty;

    public DashboardViewModel()
        : this(AppServices.SavingEntries)
    {
    }

    internal DashboardViewModel(ISavingEntryService savingEntryService)
    {
        _savingEntryService = savingEntryService;
        NewEntryCommand = new Command(async () => await Shell.Current.GoToAsync("//AddEntry"));
    }

    public decimal TotalSavings
    {
        get => _totalSavings;
        private set => SetProperty(ref _totalSavings, value);
    }

    public decimal WeeklySavings
    {
        get => _weeklySavings;
        private set => SetProperty(ref _weeklySavings, value);
    }

    public int EntryCount
    {
        get => _entryCount;
        private set => SetProperty(ref _entryCount, value);
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

    public bool HasOperationError => !string.IsNullOrWhiteSpace(OperationError);

    public ObservableCollection<DailySavingBar> LastSevenDays { get; } = [];

    public ICommand NewEntryCommand { get; }

    public async Task LoadAsync()
    {
        OperationError = string.Empty;

        try
        {
            IReadOnlyList<SavingEntry> entries = await _savingEntryService.GetAllAsync();
            DateTime today = DateTime.Today;
            int daysSinceMonday = ((int)today.DayOfWeek + 6) % 7;
            DateTime startOfWeek = today.AddDays(-daysSinceMonday);

            TotalSavings = entries.Sum(entry => entry.Amount);
            WeeklySavings = entries
                .Where(entry => entry.OccurredAt >= startOfWeek)
                .Sum(entry => entry.Amount);
            EntryCount = entries.Count;
            UpdateSevenDayChart(entries, today);
        }
        catch (Exception)
        {
            OperationError = "Die Sparübersicht konnte nicht geladen werden.";
        }
    }

    /// <summary>
    /// Gruppiert die Einträge nach Tag und skaliert den höchsten Tageswert auf 92 Pixel.
    /// Dadurch bleiben auch unterschiedlich grosse Beträge im Diagramm vergleichbar.
    /// </summary>
    private void UpdateSevenDayChart(IReadOnlyList<SavingEntry> entries, DateTime today)
    {
        DateTime firstDay = today.AddDays(-6);
        Dictionary<DateTime, decimal> totalsByDate = entries
            .Where(entry => entry.OccurredAt.Date >= firstDay && entry.OccurredAt.Date <= today)
            .GroupBy(entry => entry.OccurredAt.Date)
            .ToDictionary(group => group.Key, group => group.Sum(entry => entry.Amount));

        decimal highestAmount = totalsByDate.Count == 0 ? 0 : totalsByDate.Values.Max();
        CultureInfo swissGerman = CultureInfo.GetCultureInfo("de-CH");

        LastSevenDays.Clear();

        for (int offset = 0; offset < 7; offset++)
        {
            DateTime date = firstDay.AddDays(offset);
            decimal amount = totalsByDate.GetValueOrDefault(date);
            double height = highestAmount == 0
                ? 3
                : Math.Max(3, Math.Round((double)(amount / highestAmount) * 92));

            LastSevenDays.Add(new DailySavingBar
            {
                Date = date,
                DayLabel = date.ToString("ddd", swissGerman).TrimEnd('.'),
                Amount = amount,
                BarHeight = height
            });
        }
    }
}
