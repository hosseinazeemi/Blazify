using Microsoft.AspNetCore.Components;
using Blazify.Core.Base;
using BzCore.Enums;

namespace BzCore.Components.Navigation;

public partial class BzSidebar : BzComponentBase
{
    /// <summary>The navigation rows (BzNavItem / BzNavGroup).</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }
    /// <summary>Content at the top</summary>
    [Parameter] public RenderFragment? HeaderContent { get; set; }
    /// <summary>Content pinned to the bottom</summary>
    [Parameter] public RenderFragment? FooterContent { get; set; }

    /// <summary>Collapsed icons only, two-way bindable</summary>
    [Parameter] public bool Collapsed { get; set; }
    [Parameter] public EventCallback<bool> CollapsedChanged { get; set; }

    /// <summary>Expanded width</summary>
    [Parameter] public string Width { get; set; } = "260px";
    /// <summary>Width when collapsed</summary>
    [Parameter] public string CollapsedWidth { get; set; } = "74px";

    /// <summary>Visual style</summary>
    [Parameter] public BzSidebarVariant Variant { get; set; } = BzSidebarVariant.Default;

    /// <summary>Show built-in collapse/expand btn</summary>
    [Parameter] public bool ShowToggle { get; set; } = true;

    internal bool IsCollapsed => Collapsed;

    private string RootClass => RootCss("bz-sidebar")
        .Add($"bz-sidebar--{Variant.ToString().ToLowerInvariant()}")
        .AddIf(Collapsed, "is-collapsed")
        .Build();

    private string RootStyleStr => $"--bz-sb-w:{Width};--bz-sb-cw:{CollapsedWidth};" + (Style ?? "");

    private Task ToggleAsync() => SetCollapsed(!Collapsed);

    /// <summary>Toggle collapsed/expanded</summary>
    public Task Toggle() => SetCollapsed(!Collapsed);
    /// <summary>Expand the sidebar</summary>
    public Task Expand() => SetCollapsed(false);
    /// <summary>Collapse the sidebar</summary>
    public Task Collapse() => SetCollapsed(true);

    private async Task SetCollapsed(bool value)
    {
        Collapsed = value;
        if (CollapsedChanged.HasDelegate) await CollapsedChanged.InvokeAsync(Collapsed);
        StateHasChanged();
    }
}
