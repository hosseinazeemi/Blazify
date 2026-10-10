using Microsoft.AspNetCore.Components;
using Blazify.Core.Base;
using BzCore.Enums;

namespace BzCore.Components.Wizard;

// BzWizard — a multi-step flow. <BzStep> children register themselves (in markup
// order) via a cascading value. In Linear mode you can only reach steps up to
// _maxReached; moving forward runs the current step's OnValidate first.
public partial class BzWizard : BzComponentBase
{
    private readonly List<BzStep> _steps = new();
    private bool _dirty;
    private int _maxReached;
    private string? _message;
    private BzSeverity _messageSeverity = BzSeverity.Info;

    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public BzOrientation Orientation { get; set; } = BzOrientation.Horizontal;
    [Parameter] public BzWizardVariant Variant { get; set; } = BzWizardVariant.Numbered;

    [Parameter] public int ActiveIndex { get; set; }
    [Parameter] public EventCallback<int> ActiveIndexChanged { get; set; }

    [Parameter] public bool Linear { get; set; } = true;

    [Parameter] public RenderFragment? HeaderContent { get; set; }
    [Parameter] public RenderFragment? FooterContent { get; set; }

    [Parameter] public bool ShowDefaultButtons { get; set; } = true;
    [Parameter] public bool ShowCancel { get; set; }
    [Parameter] public string PrevText { get; set; } = "قبلی";
    [Parameter] public string NextText { get; set; } = "بعدی";
    [Parameter] public string FinishText { get; set; } = "پایان";
    [Parameter] public string CancelText { get; set; } = "انصراف";

    [Parameter] public string? Message { get; set; }
    [Parameter] public BzSeverity MessageSeverity { get; set; } = BzSeverity.Info;

    [Parameter] public EventCallback<int> OnStepChanged { get; set; }
    [Parameter] public EventCallback OnFinish { get; set; }
    [Parameter] public EventCallback OnCancel { get; set; }

    internal IReadOnlyList<BzStep> Steps => _steps;
    private BzStep? Current => ActiveIndex >= 0 && ActiveIndex < _steps.Count ? _steps[ActiveIndex] : null;
    private bool IsLast => ActiveIndex >= _steps.Count - 1;
    private bool IsFirst => ActiveIndex <= 0;
    private string? EffectiveMessage => _message ?? Message;
    private BzSeverity EffectiveSeverity => _message is not null ? _messageSeverity : MessageSeverity;

    internal void AddStep(BzStep step) { if (!_steps.Contains(step)) { _steps.Add(step); _dirty = true; } }
    internal void RemoveStep(BzStep step) { if (_steps.Remove(step)) { _dirty = true; InvokeAsync(StateHasChanged); } }

    protected override void OnAfterRender(bool firstRender)
    {
        if (_dirty) { _dirty = false; StateHasChanged(); }
    }

    private string RootClass => RootCss("bz-wizard")
        .Add($"bz-wizard--{Orientation.ToString().ToLowerInvariant()}")
        .Add($"bz-wizard--{Variant.ToString().ToLowerInvariant()}")
        .Build();

    private string StepState(int i)
    {
        if (_steps[i].Disabled) return "is-disabled";
        if (i == ActiveIndex) return "is-active";
        if (i < ActiveIndex) return "is-done";
        return "is-upcoming";
    }

    private bool CanNavigateTo(int i)
    {
        if (i < 0 || i >= _steps.Count || _steps[i].Disabled) return false;
        if (!Linear) return true;
        return i <= _maxReached;
    }

    private async Task GoToAsync(int index, bool validate = true)
    {
        if (index == ActiveIndex || index < 0 || index >= _steps.Count || _steps[index].Disabled) return;

        if (validate && index > ActiveIndex && Current?.OnValidate is not null)
        {
            var ok = await Current.OnValidate();
            if (!ok) return;
        }

        ClearMessage();
        ActiveIndex = index;
        _maxReached = Math.Max(_maxReached, index);
        if (ActiveIndexChanged.HasDelegate) await ActiveIndexChanged.InvokeAsync(index);
        if (OnStepChanged.HasDelegate) await OnStepChanged.InvokeAsync(index);
        StateHasChanged();
    }

    private async Task NextInternal()
    {
        if (IsLast) { await FinishInternal(); return; }
        var target = NextEnabledIndex(ActiveIndex, +1);
        await GoToAsync(target);
    }

    private async Task PrevInternal()
    {
        var target = NextEnabledIndex(ActiveIndex, -1);
        await GoToAsync(target, validate: false);
    }

    private async Task FinishInternal()
    {
        if (Current?.OnValidate is not null && !await Current.OnValidate()) return;
        if (OnFinish.HasDelegate) await OnFinish.InvokeAsync();
    }

    private async Task CancelInternal()
    {
        if (OnCancel.HasDelegate) await OnCancel.InvokeAsync();
    }

    // Walk from 'from' in the given direction to the first non-disabled step.
    private int NextEnabledIndex(int from, int step)
    {
        for (var i = from + step; i >= 0 && i < _steps.Count; i += step)
            if (!_steps[i].Disabled) return i;
        return from;
    }

    private async Task OnStepHeaderClick(int i)
    {
        if (CanNavigateTo(i)) await GoToAsync(i, validate: i > ActiveIndex);
    }

    private void ClearMessage() { _message = null; }

    public Task Next() => NextInternal();
    public Task Previous() => PrevInternal();
    public Task GoTo(int index) => GoToAsync(index);
    public Task Finish() => FinishInternal();

    public void SetMessage(string message, BzSeverity severity = BzSeverity.Info)
    {
        _message = message; _messageSeverity = severity; StateHasChanged();
    }
    public void ClearMessagePublic() { _message = null; StateHasChanged(); }
}
