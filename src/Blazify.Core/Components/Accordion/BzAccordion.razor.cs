using Blazify.Core.Base;
using BzCore.Enums;
using Microsoft.AspNetCore.Components;

namespace BzCore.Components.Accordion
{
    public partial class BzAccordion : BzComponentBase
    {
        private readonly List<BzAccordionItem> _items = new();
        private readonly HashSet<BzAccordionItem> _expanded = new();

        [Parameter] public RenderFragment? ChildContent { get; set; }

        [Parameter] public bool Multiple { get; set; }

        /// <summary>Layout direction.</summary>
        [Parameter] public BzOrientation Orientation { get; set; } = BzOrientation.Vertical;

        /// <summary>Visual style.</summary>
        [Parameter] public BzAccordionVariant Variant { get; set; } = BzAccordionVariant.Default;

        internal event Action? StateChanged;

        private string RootClass => RootCss("bz-acc")
            .Add($"bz-acc--{Orientation.ToString().ToLowerInvariant()}")
            .Add($"bz-acc--{Variant.ToString().ToLowerInvariant()}")
            .Build();

        // ----- called by child items -----

        internal void Register(BzAccordionItem item, bool startExpanded)
        {
            if (!_items.Contains(item)) _items.Add(item);
            if (startExpanded) _expanded.Add(item);
        }

        internal void Unregister(BzAccordionItem item)
        {
            _items.Remove(item);
            _expanded.Remove(item);
        }

        internal bool IsExpanded(BzAccordionItem item) => _expanded.Contains(item);

        internal void Toggle(BzAccordionItem item)
        {
            if (item.Disabled) return;

            if (_expanded.Contains(item))
            {
                _expanded.Remove(item);
            }
            else
            {
                if (!Multiple) _expanded.Clear();   // single open
                _expanded.Add(item);
            }

            Notify();
        }

        public void ExpandAll()
        {
            if (!Multiple) return;
            foreach (var item in _items) _expanded.Add(item);
            Notify();
        }

        /// <summary> No Need Summary ;)</summary>
        public void CollapseAll()
        {
            _expanded.Clear();
            Notify();
        }

        private void Notify()
        {
            StateChanged?.Invoke();
            StateHasChanged();
        }
    }
}
