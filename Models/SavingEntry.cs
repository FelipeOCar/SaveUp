namespace SaveUp.Models;

/// <summary>
/// Beschreibt einen einzelnen Kaufverzicht.
/// </summary>
public sealed class SavingEntry
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public string Description { get; set; } = string.Empty;

    public decimal Amount { get; set; }

    public DateTime OccurredAt { get; set; } = DateTime.Now;
}
