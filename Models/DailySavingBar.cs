namespace SaveUp.Models;

/// <summary>
/// Enthält den Tageswert und die berechnete Balkenhöhe für das 7-Tage-Diagramm.
/// </summary>
public sealed class DailySavingBar
{
    public required DateTime Date { get; init; }

    public required string DayLabel { get; init; }

    public decimal Amount { get; init; }

    public double BarHeight { get; init; }
}
