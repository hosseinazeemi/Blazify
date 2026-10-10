using Microsoft.AspNetCore.Components;
using Blazify.Core.Base;
using BzCore.Enums;

namespace BzCore.Components.Popup;

// BzPopup — a click-triggered floating panel (like a dropdown). Same open/close
// model as BzDialog: an internal _open flag that mirrors the Open parameter and
// is also driven by the trigger click and the public OpenAsync/CloseAsync API.
public partial class BzPopup : BzComponentBase
{
    private bool _open;
    private bool _lastOpenParam;

    /// <summary>Open state (two-way bindable).</summary>
    [Parameter] public bool Open { get; set; }
    [Parameter] public EventCallback<bool> OpenChanged { get; set; }

    /// <summary>The element that toggles the popup when clicked.</summary>
    [Parameter] public RenderFragment? Trigger { get; set; }
    /// <summary>The floating panel content.</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>Where the panel appears relative to the trigger.</summary>
    [Parameter] public BzPopupPlacement Placement { get; set; } = BzPopupPlacement.Bottom;
    /// <summary>Close when clicking outside the panel.</summary>
    [Parameter] public bool CloseOnOutsideClick { get; set; } = true;
    /// <summary>Fixed panel width (any CSS length).</summary>
    [Parameter] public string? Width { get; set; }

    [Parameter] public EventCallback OnOpened { get; set; }
    [Parameter] public EventCallback OnClosed { get; set; }

    private bool IsOpen => _open;
    private string PanelClass => $"bz-popup__panel bz-popup__panel--{Placement.ToString().ToLowerInvariant()}";

    protected override void OnParametersSet()
    {
        if (Open != _lastOpenParam)
        {
            _lastOpenParam = Open;
            _open = Open;
        }
    }

    private async Task SetOpenAsync(bool value)
    {
        if (_open == value) return;
        _open = value;
        _lastOpenParam = value;
        if (OpenChanged.HasDelegate) await OpenChanged.InvokeAsync(value);
        await (value ? OnOpened : OnClosed).InvokeAsync();
        StateHasChanged();
    }

    private Task ToggleInternal() => SetOpenAsync(!_open);
    private async Task OnBackdrop() { if (CloseOnOutsideClick) await SetOpenAsync(false); }

    // ----- public API -----
    /// <summary>Toggle the popup.</summary>
    public Task ToggleAsync() => SetOpenAsync(!_open);
    /// <summary>Open the popup.</summary>
    public Task OpenAsync() => SetOpenAsync(true);
    /// <summary>Close the popup.</summary>
    public Task CloseAsync() => SetOpenAsync(false);
}
