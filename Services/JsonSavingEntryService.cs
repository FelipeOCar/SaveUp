using System.Text.Json;
using Microsoft.Maui.Storage;
using SaveUp.Models;

namespace SaveUp.Services;

/// <summary>
/// Speichert alle Kaufverzichte als JSON-Datei im lokalen App-Datenordner.
/// </summary>
public sealed class JsonSavingEntryService : ISavingEntryService
{
    private readonly SemaphoreSlim _fileLock = new(1, 1);
    private readonly string _filePath;
    private readonly JsonSerializerOptions _serializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    public JsonSavingEntryService()
    {
        _filePath = Path.Combine(FileSystem.AppDataDirectory, "saving-entries.json");
    }

    public async Task<IReadOnlyList<SavingEntry>> GetAllAsync()
    {
        await _fileLock.WaitAsync();

        try
        {
            List<SavingEntry> entries = await ReadEntriesAsync();
            return entries
                .OrderByDescending(entry => entry.OccurredAt)
                .ToList();
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task<SavingEntry?> GetByIdAsync(Guid id)
    {
        await _fileLock.WaitAsync();

        try
        {
            List<SavingEntry> entries = await ReadEntriesAsync();
            return entries.FirstOrDefault(entry => entry.Id == id);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task AddAsync(SavingEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        await _fileLock.WaitAsync();

        try
        {
            List<SavingEntry> entries = await ReadEntriesAsync();
            entries.Add(entry);
            await WriteEntriesAsync(entries);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task UpdateAsync(SavingEntry entry)
    {
        ArgumentNullException.ThrowIfNull(entry);
        await _fileLock.WaitAsync();

        try
        {
            List<SavingEntry> entries = await ReadEntriesAsync();
            int index = entries.FindIndex(savedEntry => savedEntry.Id == entry.Id);

            if (index < 0)
            {
                throw new KeyNotFoundException("Der zu bearbeitende Eintrag wurde nicht gefunden.");
            }

            entries[index] = entry;
            await WriteEntriesAsync(entries);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task DeleteAsync(Guid id)
    {
        await _fileLock.WaitAsync();

        try
        {
            List<SavingEntry> entries = await ReadEntriesAsync();
            int removedEntries = entries.RemoveAll(entry => entry.Id == id);

            if (removedEntries == 0)
            {
                throw new KeyNotFoundException("Der zu löschende Eintrag wurde nicht gefunden.");
            }

            await WriteEntriesAsync(entries);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    public async Task ClearAsync()
    {
        await _fileLock.WaitAsync();

        try
        {
            await WriteEntriesAsync([]);
        }
        finally
        {
            _fileLock.Release();
        }
    }

    private async Task<List<SavingEntry>> ReadEntriesAsync()
    {
        if (!File.Exists(_filePath))
        {
            return [];
        }

        await using FileStream stream = File.OpenRead(_filePath);

        try
        {
            return await JsonSerializer.DeserializeAsync<List<SavingEntry>>(stream, _serializerOptions) ?? [];
        }
        catch (JsonException exception)
        {
            throw new InvalidDataException("Die lokale SaveUp-Datei enthält ungültige JSON-Daten.", exception);
        }
    }

    private async Task WriteEntriesAsync(IReadOnlyCollection<SavingEntry> entries)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_filePath)!);
        string temporaryPath = $"{_filePath}.tmp";

        await using (FileStream stream = File.Create(temporaryPath))
        {
            await JsonSerializer.SerializeAsync(stream, entries, _serializerOptions);
        }

        File.Move(temporaryPath, _filePath, true);
    }
}
