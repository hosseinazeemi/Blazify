using Microsoft.AspNetCore.Components;

namespace BzCore.Components.Tabs;

/// <summary>
/// A single tab. Place these inside a <see cref="BzTabs"/>; each one registers
/// itself with the parent on init and removes itself on dispose.
/// </summary>
public class BzTab : ComponentBase, IDisposable
{
    [CascadingParameter] internal BzTabs? Parent { get; set; }

    /// <summary>Tab caption.</summary>
    [Parameter] public string? Title { get; set; }
    /// <summary>Optional icon (font-awesome class).</summary>
    [Parameter] public string? Icon { get; set; }
    /// <summary>Disable this tab.</summary>
    [Parameter] public bool Disabled { get; set; }
    /// <summary>Optional small badge (e.g. a count) shown on the tab.</summary>
    [Parameter] public string? Badge { get; set; }
    /// <summary>The panel content shown when this tab is active.</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    protected override void OnInitialized() => Parent?.AddTab(this);
    public void Dispose() => Parent?.RemoveTab(this);
}
