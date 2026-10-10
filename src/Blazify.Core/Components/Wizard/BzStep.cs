using Microsoft.AspNetCore.Components;

namespace BzCore.Components.Wizard;

/// <summary>
/// One step of a <see cref="BzWizard"/>. Steps register themselves with the parent
/// wizard on init (and unregister on dispose), so their order follows markup order.
/// </summary>
public class BzStep : ComponentBase, IDisposable
{
    [CascadingParameter] internal BzWizard? Parent { get; set; }

    /// <summary>Step title shown in the header.</summary>
    [Parameter] public string? Title { get; set; }
    /// <summary>Secondary text under the title.</summary>
    [Parameter] public string? Subtitle { get; set; }
    /// <summary>Optional icon (font-awesome class).</summary>
    [Parameter] public string? Icon { get; set; }
    /// <summary>Disable navigation to this step.</summary>
    [Parameter] public bool Disabled { get; set; }
    /// <summary>Mark the step as optional (for display).</summary>
    [Parameter] public bool Optional { get; set; }

    /// <summary>Runs before leaving this step forward; return false to block navigation.</summary>
    [Parameter] public Func<Task<bool>>? OnValidate { get; set; }

    /// <summary>Step body.</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    protected override void OnInitialized() => Parent?.AddStep(this);
    public void Dispose() => Parent?.RemoveStep(this);
}
