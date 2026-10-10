using BzCore.Enums;
using BzCore.Validation;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace Blazify.Core.Base
{
    public abstract class BzFieldBase<TBind> : BzComponentBase
    {
        [CascadingParameter]
        protected EditContext? EditContext { get; set; }

        [CascadingParameter]
        protected IBzFormContext? FormContext { get; set; }

        protected FieldIdentifier FieldId;

        private bool _registered;
        private string? _registeredGroup;
        private bool _disabledOverride, _readOnlyOverride, _hasDisabled, _hasReadOnly;

        [Parameter]
        public TBind? Value { get; set; }

        [Parameter]
        public EventCallback<TBind?> ValueChanged { get; set; }

        [Parameter]
        public Expression<Func<TBind?>>? ValueExpression { get; set; }

        [Parameter]
        public string? Name { get; set; }

        [Parameter]
        public string? DisplayName { get; set; }

        [Parameter]
        public string? Label { get; set; }

        [Parameter]
        public string? Placeholder { get; set; }

        [Parameter]
        public string? HelperText { get; set; }

        [Parameter]
        public string? StartIcon { get; set; }

        [Parameter]
        public string? EndIcon { get; set; }

        [Parameter]
        public BzSize Size { get; set; } = BzSize.Medium;

        [Parameter]
        public bool Disabled { get; set; }

        [Parameter]
        public bool ReadOnly { get; set; }

        [Parameter]
        public bool Required { get; set; }

        [Parameter]
        public bool Clearable { get; set; }

        [Parameter]
        public int TabIndex { get; set; }

        [Parameter]
        public string? ValidationGroup { get; set; }

        protected bool IsDisabled => _hasDisabled ? _disabledOverride : Disabled;

        protected bool IsReadOnly => _hasReadOnly ? _readOnlyOverride : ReadOnly;

        protected string EffectiveName => Name ?? Id;

        protected bool HasError =>
            EditContext is not null &&
            ValueExpression is not null &&
            EditContext.GetValidationMessages(FieldId).Any();

        protected string? ErrorText =>
            HasError
                ? EditContext!.GetValidationMessages(FieldId).FirstOrDefault()
                : null;

        protected IReadOnlyDictionary<string, object>? SafeAttributes
        {
            get
            {
                if (AdditionalAttributes is null)
                    return null;

                var d = new Dictionary<string, object>(AdditionalAttributes);
                d.Remove("class");
                d.Remove("style");
                return d;
            }
        }

        protected override void OnParametersSet()
        {
            if (ValueExpression is not null)
                FieldId = FieldIdentifier.Create(ValueExpression);

            if (FormContext is not null &&
                ValueExpression is not null &&
                (!_registered || _registeredGroup != ValidationGroup))
            {
                if (_registered)
                    FormContext.UnregisterField(FieldId);

                FormContext.RegisterField(FieldId, ValidationGroup);
                _registered = true;
                _registeredGroup = ValidationGroup;
            }

            OnFieldParametersSet();
        }

        protected virtual void OnFieldParametersSet() { }

        protected async Task SetValueAsync(TBind? value)
        {
            Value = value;
            await ValueChanged.InvokeAsync(value);

            if (ValueExpression is not null)
                EditContext?.NotifyFieldChanged(FieldId);
        }

        protected void NotifyFieldChanged()
        {
            if (ValueExpression is not null)
                EditContext?.NotifyFieldChanged(FieldId);
        }

        public void SetDisabled(bool value)
        {
            _hasDisabled = true;
            _disabledOverride = value;
            StateHasChanged();
        }

        public void SetReadOnly(bool value)
        {
            _hasReadOnly = true;
            _readOnlyOverride = value;
            StateHasChanged();
        }

        public override void Dispose()
        {
            if (_registered && FormContext is not null)
                FormContext.UnregisterField(FieldId);

            base.Dispose();
        }
    }
}
