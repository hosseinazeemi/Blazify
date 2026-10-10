using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Microsoft.JSInterop;
using BzCore.Enums;
using BzCore.Helpers;
using Blazify.Core.Base;

namespace BzCore.Components.Button;

public partial class BzButton : BzComponentBase
{
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private ElementReference _element;

    private bool _busyOverride, _hasBusyOverride;
    private bool _disabledOverride, _hasDisabledOverride;
    private bool _readOnlyOverride, _hasReadOnlyOverride;
    private string? _textOverride;

    [Parameter] public string? Text { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }

    [Parameter] public BzColor Color { get; set; } = BzColor.Primary;
    [Parameter] public BzButtonVariant Variant { get; set; } = BzButtonVariant.Filled;
    [Parameter] public BzSize Size { get; set; } = BzSize.Medium;

    [Parameter] public string? StartIcon { get; set; }
    [Parameter] public string? EndIcon { get; set; }
    [Parameter] public bool IconOnly { get; set; }
    [Parameter] public bool FullWidth { get; set; }
    [Parameter] public bool Rounded { get; set; }

    [Parameter] public bool Disabled { get; set; }
    [Parameter] public bool ReadOnly { get; set; }

    /// <summary>
    /// use @bind-Loading for best results. age ino didi bedoon service shodam xD, Halesho Bebarin
    /// </summary>
    [Parameter] public bool Loading { get; set; }
    [Parameter] public EventCallback<bool> LoadingChanged { get; set; }

    [Parameter] public BzLoadingEffect LoadingEffect { get; set; } = BzLoadingEffect.Spinner;

    [Parameter] public bool AutoLoadingOnClick { get; set; } = true;

    [Parameter] public bool Ripple { get; set; } = true;

    [Parameter] public int TabIndex { get; set; }
    // submit,button,reset
    [Parameter] public string ButtonType { get; set; } = "button";

    // on click event callback
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }

    private bool IsBusy => _hasBusyOverride ? _busyOverride : Loading;
    private bool IsDisabled => _hasDisabledOverride ? _disabledOverride : Disabled;
    private bool IsReadOnly => _hasReadOnlyOverride ? _readOnlyOverride : ReadOnly;
    private string? EffectiveText => _textOverride ?? Text;
    private string HtmlType => ButtonType;

    private string ButtonClass => RootCss("bz-btn")
        .Add($"bz-btn--{Color.ToString().ToLowerInvariant()}")
        .Add($"bz-btn--{Variant.ToString().ToLowerInvariant()}")
        .Add($"bz-btn--{Size.ToString().ToLowerInvariant()}")
        .AddIf(FullWidth, "bz-btn--block")
        .AddIf(Rounded, "bz-btn--rounded")
        .AddIf(IconOnly, "bz-btn--icon-only")
        .AddIf(IsBusy, "is-busy")
        .AddIf(IsReadOnly, "is-readonly")
        .Build();

    private IReadOnlyDictionary<string, object>? SafeAttributes
    {
        get
        {
            if (AdditionalAttributes is null)
                return null;
            var dict = new Dictionary<string, object>(AdditionalAttributes);
            dict.Remove("class");
            dict.Remove("style");
            return dict;
        }
    }

    private async Task HandleClickAsync(MouseEventArgs args)
    {
        if (IsDisabled || IsBusy || IsReadOnly)
            return;

        if (Ripple)
        {
            try
            {
                await JS.InvokeVoidAsync("bzButton.ripple", _element, args.ClientX, args.ClientY);
            }
            catch
            {
                /* JS not loaded yet (e.g. prerender) – ignore */
            }
        }

        if (!OnClick.HasDelegate)
            return;

        if (AutoLoadingOnClick)
        {
            await SetLoadingAsync(true);
            try
            {
                await OnClick.InvokeAsync(args);
            }
            finally
            {
                await SetLoadingAsync(false);
            }
        }
        else
        {
            await OnClick.InvokeAsync(args);
        }
    }

    private async Task SetLoadingAsync(bool value)
    {
        _hasBusyOverride = true;
        _busyOverride = value;
        if (LoadingChanged.HasDelegate) 
            await LoadingChanged.InvokeAsync(value);
        StateHasChanged();
    }

    // ----- public with @ref -----

    public void ShowSpinner() => SetLoading(true);
    public void HideSpinner() => SetLoading(false);
    public void SetLoading(bool value) { _hasBusyOverride = true; _busyOverride = value; StateHasChanged(); }

    public void Disable() => SetDisabled(true);
    public void Enable() => SetDisabled(false);
    public void SetDisabled(bool value) { _hasDisabledOverride = true; _disabledOverride = value; StateHasChanged(); }

    public void SetReadOnly(bool value) { _hasReadOnlyOverride = true; _readOnlyOverride = value; StateHasChanged(); }

    public void SetText(string text) { _textOverride = text; StateHasChanged(); }

    public void ResetOverrides()
    {
        _hasBusyOverride = _hasDisabledOverride = _hasReadOnlyOverride = false;
        _textOverride = null;
        StateHasChanged();
    }
}
