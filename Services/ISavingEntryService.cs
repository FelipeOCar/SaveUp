using SaveUp.Models;

namespace SaveUp.Services;

public interface ISavingEntryService
{
    Task<IReadOnlyList<SavingEntry>> GetAllAsync();

    Task<SavingEntry?> GetByIdAsync(Guid id);

    Task AddAsync(SavingEntry entry);

    Task UpdateAsync(SavingEntry entry);

    Task DeleteAsync(Guid id);

    Task ClearAsync();
}
