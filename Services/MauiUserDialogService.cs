namespace SaveUp.Services;

public sealed class MauiUserDialogService : IUserDialogService
{
    public Task<bool> ConfirmAsync(string title, string message, string accept, string cancel)
    {
        return Shell.Current.DisplayAlert(title, message, accept, cancel);
    }

    public Task<string?> ChooseActionAsync(string title, string cancel, params string[] actions)
    {
        return Shell.Current.DisplayActionSheet(title, cancel, null, actions);
    }
}
