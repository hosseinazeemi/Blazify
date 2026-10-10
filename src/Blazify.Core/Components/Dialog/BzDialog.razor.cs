using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using Blazify.Core.Base;
using BzCore.Enums;
using BzCore.Dialog;

namespace BzCore.Components.Dialog;

public partial class BzDialog : BzComponentBase
{
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private bool _open;
    private bool _lastOpenParam;   // last Open value
    private bool _scrollLocked;
    private readonly HashSet<BzDialogButton> _busyButtons = new();

    [Parameter] public string? Title { get; set; }
    [Parameter] public string? Icon { get; set; }
    [Parameter] public BzColor IconColor { get; set; } = BzColor.Primary;

    /// <summary>
    /// two-way binding parameter
    /// </summary>
    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }

    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public RenderFragment? HeaderContent { get; set; }
    [Parameter] public RenderFragment? FooterContent { get; set; }

    /// <summary>used when FooterContent is null</summary>
    [Parameter] public List<BzDialogButton>? Buttons { get; set; }

    [Parameter] public BzDialogSize Size { get; set; } = BzDialogSize.Medium;
    [Parameter] public bool ShowHeader { get; set; } = true;
    [Parameter] public bool ShowCloseButton { get; set; } = true;
    [Parameter] public bool CloseOnBackdrop { get; set; } = true;
    [Parameter] public bool CloseOnEsc { get; set; } = true;

    [Parameter] public EventCallback<BzDialogResult> OnClose { get; set; }
    [Parameter] public EventCallback OnOpened { get; set; }

    private bool IsOpen => _open;

    private string BoxClass => $"bz-dialog__box bz-dialog__box--{Size.ToString().ToLowerInvariant()}";
    private string IconColorClass => $"bz-dialog__icon--{IconColor.ToString().ToLowerInvariant()}";

    protected override void OnParametersSet()
    {
        // two-way check Open with _lastOpenParam
        if (Open != _lastOpenParam)
        {
            _lastOpenParam = Open;
            _open = Open;
            if (_open) _ = OnOpened.InvokeAsync();
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_open && !_scrollLocked)
        {
            _scrollLocked = true;
            await SafeJs("bzDialog.lock");
        }
        else if (!_open && _scrollLocked)
        {
            _scrollLocked = false;
            await SafeJs("bzDialog.unlock");
        }
    }

    private async Task SafeJs(string fn)
    {
        try { await JS.InvokeVoidAsync(fn); } catch { /* prerender JS not ready */ }
    }

    private async Task SetOpenAsync(bool value)
    {
        if (_open == value) return;
        _open = value;
        _lastOpenParam = value;
        if (OpenChanged.HasDelegate) 
            await OpenChanged.InvokeAsync(value);
        if (value) 
            await OnOpened.InvokeAsync();
        StateHasChanged();
    }

    private async Task CloseWith(BzDialogResult result)
    {
        await SetOpenAsync(false);
        if (OnClose.HasDelegate) 
            await OnClose.InvokeAsync(result);
    }

    private async Task OnBackdropClick()
    {
        if (CloseOnBackdrop) 
            await CloseWith(BzDialogResult.Cancelled());
    }

    private async Task OnKeyDown(KeyboardEventArgs e)
    {
        if (CloseOnEsc && e.Key == "Escape") 
            await CloseWith(BzDialogResult.Cancelled());
    }

    // Runs a footer button's handler (showing a spinner if asked) then optionally closes.
    private async Task ClickButtonAsync(BzDialogButton button)
    {
        if (button.OnClick is not null)
        {
            if (button.ShowLoading) { _busyButtons.Add(button); StateHasChanged(); }
            try { await button.OnClick(); }
            finally { _busyButtons.Remove(button); }
        }

        if (button.CloseOnClick)
            await CloseWith(new BzDialogResult { Confirmed = !button.IsCancel, Data = button.ReturnValue });
        else
            StateHasChanged();
    }

    private bool IsButtonBusy(BzDialogButton b) => _busyButtons.Contains(b);

    // ----- public API -----
    /// <summary>Open the dialog.</summary>
    public Task Show() => SetOpenAsync(true);
    /// <summary>Close the dialog with a "cancelled" result.</summary>
    public Task Close() => CloseWith(BzDialogResult.Cancelled());
    /// <summary>Close the dialog with a specific result.</summary>
    public Task CloseWithResult(BzDialogResult result) => CloseWith(result);

    public override void Dispose()
    {
        if (_scrollLocked)
        {
            _scrollLocked = false;
            try { _ = JS.InvokeVoidAsync("bzDialog.unlock"); } catch { /* ignore */ }
        }
        base.Dispose();
    }
}
