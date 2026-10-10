using Microsoft.AspNetCore.Components;

namespace BzCore.Dialog;

public interface IBzDialogService
{
    event Action? Changed;
    IReadOnlyList<BzDialogReference> Dialogs { get; }

    BzDialogReference Show(BzDialogOptions options);
    Task<BzDialogResult> ShowAsync(BzDialogOptions options);

    Task<bool> ConfirmAsync(string title, string message, string okText = "تایید", string cancelText = "انصراف");

    Task AlertAsync(string title, string message, string okText = "باشه");

    void Close(BzDialogReference dialog, BzDialogResult result);
    void CloseAll();
}

public sealed class BzDialogService : IBzDialogService
{
    private readonly List<BzDialogReference> _dialogs = new();

    public event Action? Changed;
    public IReadOnlyList<BzDialogReference> Dialogs => _dialogs;

    public BzDialogReference Show(BzDialogOptions options)
    {
        var reference = new BzDialogReference(options);
        _dialogs.Add(reference);
        Changed?.Invoke();
        return reference;
    }

    public Task<BzDialogResult> ShowAsync(BzDialogOptions options) => Show(options).Result;

    public Task<bool> ConfirmAsync(string title, string message, string okText = "تایید", string cancelText = "انصراف")
    {
        var options = new BzDialogOptions
        {
            Title = title,
            Message = message,
            Icon = "fa-solid fa-circle-question",
            Buttons =
            {
                BzDialogButton.Cancel(cancelText),
                BzDialogButton.Ok(okText)
            }
        };
        return ShowAsync(options).ContinueWith(t => t.Result.Confirmed);
    }

    public Task AlertAsync(string title, string message, string okText = "باشه")
    {
        var options = new BzDialogOptions
        {
            Title = title,
            Message = message,
            Icon = "fa-solid fa-circle-info",
            Buttons = { BzDialogButton.Ok(okText) }
        };
        return ShowAsync(options);
    }

    public void Close(BzDialogReference dialog, BzDialogResult result)
    {
        if (_dialogs.Remove(dialog))
        {
            dialog.Completion.TrySetResult(result);
            Changed?.Invoke();
        }
    }

    public void CloseAll()
    {
        foreach (var d in _dialogs.ToList())
            d.Completion.TrySetResult(BzDialogResult.Cancelled());
        _dialogs.Clear();
        Changed?.Invoke();
    }
}
