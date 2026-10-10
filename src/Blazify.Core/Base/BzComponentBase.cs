using BzCore.Helpers;
using BzCore.Theming;
using Microsoft.AspNetCore.Components;
using System;
using System.Collections.Generic;
using System.Reflection.Emit;

namespace Blazify.Core.Base
{
    public abstract class BzComponentBase : ComponentBase, IDisposable
    {
        private bool _subscribed;

        [Inject] protected IBzThemeService ThemeService { get; set; } = default!;

        [Parameter] public string Id { get; set; } = IdentifierHelper.New();

        [Parameter] public string? Class { get; set; }

        [Parameter] public string? Style { get; set; }

        [Parameter] public bool Visible { get; set; } = true;

        /// <summary>Overrides the global compact mode for this component.</summary>
        [Parameter] public bool Dense { get; set; } = true;

        protected bool IsDense => Dense && ThemeService.Dense;

        [Parameter(CaptureUnmatchedValues = true)]
        public IReadOnlyDictionary<string, object>? AdditionalAttributes { get; set; }

        protected override void OnInitialized()
        {
            ThemeService.Changed += OnThemeChanged;
            _subscribed = true;
        }

        private void OnThemeChanged() => InvokeAsync(StateHasChanged);

        protected CssBuilder RootCss(string baseClass) =>
            CssBuilder.Default(baseClass).AddIf(IsDense, "bz-dense").Add(Class).AddFromAttributes(AdditionalAttributes);

        protected StyleBuilder RootStyle() =>
            StyleBuilder.Default(Style).AddFromAttributes(AdditionalAttributes);

        public virtual void Dispose()
        {
            if (_subscribed) ThemeService.Changed -= OnThemeChanged;
            GC.SuppressFinalize(this);
        }
    }
}
