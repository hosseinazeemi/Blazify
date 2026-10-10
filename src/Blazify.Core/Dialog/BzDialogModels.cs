using BzCore.Enums;
using System;
using System.Threading.Tasks;

namespace BzCore.Dialog;

public sealed class BzDialogButton
{
    public string Text { get; set; } = "تایید";
    public string? Icon { get; set; }
    public BzColor Color { get; set; } = BzColor.Primary;
    public BzButtonVariant Variant { get; set; } = BzButtonVariant.Filled;

    public bool CloseOnClick { get; set; } = true;

    public bool IsCancel { get; set; }

    public bool ShowLoading { get; set; } = true;

    public Func<Task>? OnClick { get; set; }

    public object? ReturnValue { get; set; }

    public static BzDialogButton Ok(string text = "تایید", object? value = null) =>
        new() { Text = text, Color = BzColor.Primary, ReturnValue = value };

    public static BzDialogButton Cancel(string text = "انصراف") =>
        new() { Text = text, Color = BzColor.Default, Variant = BzButtonVariant.Text, IsCancel = true };

    public static BzDialogButton Danger(string text, object? value = null) =>
        new() { Text = text, Color = BzColor.Danger, ReturnValue = value };
}

public sealed class BzDialogResult
{
    public bool Confirmed { get; init; }
    public object? Data { get; init; }

    public static BzDialogResult Ok(object? data = null) => new()
    {
        Confirmed = true,
        Data = data
    };
    public static BzDialogResult Cancelled() => new()
    {
        Confirmed = false
    };
}
