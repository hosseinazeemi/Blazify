using Microsoft.AspNetCore.Components;
using BzCore.Enums;

namespace BzCore.Dialog;

public sealed class BzDialogOptions
{
    public string? Title { get; set; }

    public RenderFragment? Content { get; set; }

    public string? Message { get; set; }

    public List<BzDialogButton> Buttons { get; set; } = new();

    public BzDialogSize Size { get; set; } = BzDialogSize.Medium;
    public bool CloseOnBackdrop { get; set; } = true;
    public bool ShowCloseButton { get; set; } = true;
    public string? Icon { get; set; }
    public BzColor IconColor { get; set; } = BzColor.Primary;
}

public sealed class BzDialogReference
{
    public Guid Id { get; } = Guid.NewGuid();
    public BzDialogOptions Options { get; }
    internal TaskCompletionSource<BzDialogResult> Completion { get; } =
        new(TaskCreationOptions.RunContinuationsAsynchronously);

    public Task<BzDialogResult> Result => Completion.Task;

    public BzDialogReference(BzDialogOptions options) => Options = options;
}
