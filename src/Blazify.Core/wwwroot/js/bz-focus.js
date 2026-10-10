/* ------------------------------------------------------------------
   bz-focus.js — configurable focus navigation.
   Lets you advance focus to the next element (by tabindex order)
   with a custom key, e.g. Enter instead of (or in addition to) Tab.
   Public API: window.bzFocusNav.attach(root, options) / .detach(root)
   ------------------------------------------------------------------ */
(function () {
    "use strict";

    function isVisible(el) {
        if (el.disabled || el.getAttribute("aria-hidden") === "true") return false;
        return !!(el.offsetWidth || el.offsetHeight || el.getClientRects().length) &&
            getComputedStyle(el).visibility !== "hidden";
    }

    // Focusable elements in *tabindex order*: positive tabindex first
    // (ascending), then natural order (tabindex 0 / no tabindex).
    function focusables(root) {
        var sel = 'input:not([disabled]):not([type="hidden"]),' +
                  'select:not([disabled]),textarea:not([disabled]),' +
                  'button:not([disabled]),a[href],[tabindex]';
        return Array.prototype.slice.call(root.querySelectorAll(sel))
            .filter(function (el) { return el.tabIndex >= 0 && isVisible(el); })
            .map(function (el, i) { return { el: el, i: i, ti: el.tabIndex }; })
            .sort(function (a, b) {
                var at = a.ti === 0 ? Number.MAX_SAFE_INTEGER : a.ti;
                var bt = b.ti === 0 ? Number.MAX_SAFE_INTEGER : b.ti;
                return (at - bt) || (a.i - b.i);   // stable within equal tabindex
            })
            .map(function (x) { return x.el; });
    }

    var registry = new WeakMap();

    function attach(root, opts) {
        if (!root || registry.has(root)) return;
        opts = opts || {};
        var advance = (opts.advanceKeys && opts.advanceKeys.length ? opts.advanceKeys : ["Enter"])
            .map(function (k) { return String(k).toLowerCase(); });
        var reverse = (opts.reverseKeys || []).map(function (k) { return String(k).toLowerCase(); });
        var loop = opts.loop !== false;            // default true
        var skipTextarea = opts.skipTextarea !== false; // default true

        function handler(e) {
            var key = (e.key || "").toLowerCase();
            var isAdv = advance.indexOf(key) !== -1;
            var isRev = reverse.indexOf(key) !== -1;
            if (!isAdv && !isRev) return;

            // Let Enter type a newline inside a textarea.
            if (skipTextarea && key === "enter" && e.target && e.target.tagName === "TEXTAREA") return;
            // Don't hijack combos we don't manage (Shift is used for reverse).
            if (e.ctrlKey || e.altKey || e.metaKey) return;

            var goBack = isRev || (isAdv && e.shiftKey);
            var list = focusables(root);
            if (!list.length) return;

            var idx = list.indexOf(document.activeElement);
            var next;
            if (idx === -1) {
                next = 0;
            } else {
                next = idx + (goBack ? -1 : 1);
                if (next < 0) next = loop ? list.length - 1 : 0;
                if (next >= list.length) next = loop ? 0 : list.length - 1;
            }

            var target = list[next];
            if (target) {
                e.preventDefault();
                target.focus();
                if (typeof target.select === "function") { try { target.select(); } catch (_) { } }
            }
        }

        root.addEventListener("keydown", handler, true);
        registry.set(root, handler);
    }

    function detach(root) {
        if (!root) return;
        var h = registry.get(root);
        if (h) { root.removeEventListener("keydown", h, true); registry.delete(root); }
    }

    window.bzFocusNav = { attach: attach, detach: detach };
})();
