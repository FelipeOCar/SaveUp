namespace SaveUp.Services;

public interface IUserDialogService
{
    Task<bool> ConfirmAsync(string title, string message, string accept, string cancel);

    Task<string?> ChooseActionAsync(string title, string cancel, params string[] actions);
}
