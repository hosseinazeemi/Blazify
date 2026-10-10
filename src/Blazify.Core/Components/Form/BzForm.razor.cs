using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;
using Blazify.Core.Base;
using BzCore.Enums;
using BzCore.Validation;

namespace BzCore.Components.Form;

// BzForm<TModel> — a validated form built on Blazor's EditContext.
//   • Fields register themselves through IBzFormContext (RegisterField), tagging an
//     optional 'group' so a subset can be validated on its own (ValidateGroup).
//   • Validation runs through BzValidationEngine (DataAnnotations + your custom rules)
//     and the results are pushed into a ValidationMessageStore so the standard Blazor
//     validation UI lights up.
//   • On submit it validates, then calls OnValidSubmit / OnInvalidSubmit, optionally
//     showing a loading overlay for the duration.
public partial class BzForm<TModel> : BzComponentBase, IBzFormContext
{
    [Inject] private IJSRuntime JS { get; set; } = default!;

    private EditContext _editContext = default!;
    private ValidationMessageStore _store = default!;
    private TModel? _model;
    private ElementReference _formEl;
    private bool _busyOverride, _hasBusyOverride;

    private readonly List<(FieldIdentifier field, string? group)> _fields = new();

    /// <summary>The model instance being edited. Required.</summary>
    [Parameter, EditorRequired] public TModel Model { get; set; } = default!;
    /// <summary>The form fields.</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>Which validation sources to run (DataAnnotations and/or Custom).</summary>
    [Parameter] public BzValidationMode ValidationMode { get; set; } = BzValidationMode.DataAnnotations;

    /// <summary>Optional external validator instance (reserved for FluentValidation-style use).</summary>
    [Parameter] public object? Validator { get; set; }

    /// <summary>Your own rules; return one error per problem.</summary>
    [Parameter] public Func<TModel, IEnumerable<BzValidationError>>? CustomValidator { get; set; }

    /// <summary>Re-validate a field as soon as it changes.</summary>
    [Parameter] public bool LiveValidation { get; set; } = true;

    [Parameter] public EventCallback<TModel> OnValidSubmit { get; set; }
    [Parameter] public EventCallback<TModel> OnInvalidSubmit { get; set; }
    [Parameter] public EventCallback<TModel> OnSubmit { get; set; }

    /// <summary>Show the loading overlay while OnValidSubmit runs.</summary>
    [Parameter] public bool AutoLoadingOnSubmit { get; set; } = true;
    [Parameter] public bool Loading { get; set; }
    [Parameter] public BzLoadingEffect LoadingEffect { get; set; } = BzLoadingEffect.Spinner;

    /// <summary>Show a validation summary at the top of the form.</summary>
    [Parameter] public bool ShowSummary { get; set; }

    public EditContext EditContext => _editContext;

    public void RegisterField(FieldIdentifier field, string? group)
    {
        if (!_fields.Any(f => f.field.Equals(field)))
            _fields.Add((field, group));
    }

    public void UnregisterField(FieldIdentifier field)
        => _fields.RemoveAll(f => f.field.Equals(field));

    private bool IsBusy => _hasBusyOverride ? _busyOverride : Loading;
    private string FormClass => RootCss("bz-form").AddIf(IsBusy, "is-busy").Build();
    private IEnumerable<string> AllMessages => _editContext.GetValidationMessages();

    protected override void OnParametersSet()
    {
        // When the Model instance changes, tear down the old EditContext and build a
        // fresh one bound to the new model (re-wiring the validation/field-change hooks).
        if (!ReferenceEquals(_model, Model))
        {
            if (_editContext is not null)
            {
                _editContext.OnValidationRequested -= HandleValidationRequested;
                _editContext.OnFieldChanged -= HandleFieldChanged;
            }

            _model = Model;
            _editContext = new EditContext(Model!);
            _store = new ValidationMessageStore(_editContext);
            _editContext.OnValidationRequested += HandleValidationRequested;
            _editContext.OnFieldChanged += HandleFieldChanged;
        }
    }

    private void HandleValidationRequested(object? sender, ValidationRequestedEventArgs e)
        => RunValidation(group: null);

    private void HandleFieldChanged(object? sender, FieldChangedEventArgs e)
    {
        if (LiveValidation) RevalidateField(e.FieldIdentifier);
    }

    private Func<object, IEnumerable<BzValidationError>>? BuildCustom()
        => CustomValidator is null ? null : (m => CustomValidator((TModel)m));

    private bool RunValidation(string? group)
    {
        var errors = BzValidationEngine.Validate(Model!, ValidationMode, Validator, BuildCustom());

        if (group is null)
        {
            _store.Clear();
            foreach (var err in errors)
                _store.Add(ToField(err.FieldName), err.Message);
            _editContext.NotifyValidationStateChanged();
            return !errors.Any();
        }

        // Group validation: keep only the errors whose field belongs to this group.
        var groupNames = _fields.Where(f => f.group == group)
                                .Select(f => f.field.FieldName)
                                .ToHashSet(StringComparer.Ordinal);

        foreach (var f in _fields.Where(f => f.group == group))
            _store.Clear(f.field);

        var groupErrors = errors.Where(e => groupNames.Contains(e.FieldName)).ToList();
        foreach (var err in groupErrors)
            _store.Add(ToField(err.FieldName), err.Message);

        _editContext.NotifyValidationStateChanged();
        return groupErrors.Count == 0;
    }

    private void RevalidateField(FieldIdentifier field)
    {
        var errors = BzValidationEngine.Validate(Model!, ValidationMode, Validator, BuildCustom());

        _store.Clear(field);
        foreach (var err in errors.Where(e => string.Equals(e.FieldName, field.FieldName, StringComparison.Ordinal)))
            _store.Add(field, err.Message);

        _editContext.NotifyValidationStateChanged();
    }

    private FieldIdentifier ToField(string fieldName)
        => string.IsNullOrEmpty(fieldName) ? new FieldIdentifier(Model!, string.Empty) : _editContext.Field(fieldName);

    private async Task HandleSubmitAsync()
    {
        if (OnSubmit.HasDelegate) await OnSubmit.InvokeAsync(Model);

        var valid = RunValidation(group: null);
        if (!valid)
        {
            await OnInvalidSubmit.InvokeAsync(Model);
            return;
        }

        if (AutoLoadingOnSubmit) await ShowLoadingAsync();
        try { await OnValidSubmit.InvokeAsync(Model); }
        finally { if (AutoLoadingOnSubmit) await HideLoadingAsync(); }
    }

    private async Task ShowLoadingAsync()
    {
        _hasBusyOverride = true; _busyOverride = true; StateHasChanged();
        try { await JS.InvokeVoidAsync("bzCore.showLoading", _formEl, LoadingEffect.ToString().ToLowerInvariant()); }
        catch { /* prerender */ }
    }

    private async Task HideLoadingAsync()
    {
        _hasBusyOverride = true; _busyOverride = false; StateHasChanged();
        try { await JS.InvokeVoidAsync("bzCore.hideLoading", _formEl); }
        catch { /* prerender */ }
    }

    public bool Validate() => _editContext.Validate();

    public bool ValidateGroup(string groupKey) => RunValidation(groupKey);

    public bool IsValid() => !_editContext.GetValidationMessages().Any();

    public void ClearValidation()
    {
        _store.Clear();
        _editContext.NotifyValidationStateChanged();
    }

    public Task SubmitAsync() => HandleSubmitAsync();

    public void SetLoading(bool value)
    {
        _hasBusyOverride = true; _busyOverride = value; StateHasChanged();
        _ = value ? ShowLoadingAsync() : HideLoadingAsync();
    }

    public override void Dispose()
    {
        if (_editContext is not null)
        {
            _editContext.OnValidationRequested -= HandleValidationRequested;
            _editContext.OnFieldChanged -= HandleFieldChanged;
        }
        base.Dispose();
    }
}
