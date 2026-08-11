namespace SaveUp.Services;

/// <summary>
/// Stellt die gemeinsam genutzten App-Dienste für die ViewModels bereit.
/// </summary>
public static class AppServices
{
    public static ISavingEntryService SavingEntries { get; } = new JsonSavingEntryService();

    public static IUserDialogService Dialogs { get; } = new MauiUserDialogService();
}
