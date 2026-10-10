// bz-input.js — keeps an input's DOM value in sync with the filtered value
// computed in C#. Needed because Blazor skips writing <input value> when the
// new render value equals the previous render value.
window.bzInput = (function () {
    function setValue(el, value, caret) {
        if (!el) return;
        value = value ?? '';

        // Explicit caret (mask/money): C# knows the formatted length, so it
        // tells us exactly where the caret should sit (avoids separator drift).
        if (typeof caret === 'number' && caret >= 0) {
            el.value = value;
            var c = caret; if (c > value.length) c = value.length; if (c < 0) c = 0;
            try { el.setSelectionRange(c, c); } catch (_) { }
            return;
        }

        if (el.value === value) return;

        // Preserve a sensible caret position after filtering.
        var prev = el.value;
        var start = el.selectionStart ?? value.length;

        // How many chars were removed before the caret?
        var removed = prev.length - value.length;
        var caret2 = start - (removed > 0 ? removed : 0);
        if (caret2 < 0) caret2 = 0;
        if (caret2 > value.length) caret2 = value.length;

        el.value = value;
        try { el.setSelectionRange(caret2, caret2); } catch (_) { /* number inputs */ }
    }

    return { setValue };
})();
