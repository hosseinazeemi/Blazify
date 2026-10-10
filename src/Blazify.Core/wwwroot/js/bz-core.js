// bz-core.js — shared low-level helpers exposed as window.bzCore.
window.bzCore = (function () {

    // Smooth show/hide of a loading overlay over any element (used by bzForm).
    function showLoading(el, effect) {
        if (!el || el.querySelector(':scope > .bz-loading-overlay')) return;
        const ov = document.createElement('div');
        ov.className = 'bz-loading-overlay bz-loading--' + (effect || 'spinner');
        ov.innerHTML = spinnerMarkup(effect);
        const pos = getComputedStyle(el).position;
        if (pos === 'static') el.style.position = 'relative';
        el.appendChild(ov);
        requestAnimationFrame(() => ov.classList.add('is-visible'));
    }

    function hideLoading(el) {
        if (!el) return;
        const ov = el.querySelector(':scope > .bz-loading-overlay');
        if (!ov) return;
        ov.classList.remove('is-visible');
        ov.addEventListener('transitionend', () => ov.remove(), { once: true });
        setTimeout(() => ov.remove(), 400);
    }

    function spinnerMarkup(effect) {
        switch (effect) {
            case 'dots': return '<div class="bz-l-dots"><i></i><i></i><i></i></div>';
            case 'bar': return '<div class="bz-l-bar"><span></span></div>';
            case 'pulse': return '<div class="bz-l-pulse"></div>';
            default: return '<div class="bz-l-spinner"></div>';
        }
    }

    function focus(el) { if (el) el.focus(); }

    // Keep the caret sensible after re-formatting a money input.
    function setCaretEnd(el) {
        if (el && typeof el.selectionStart === 'number') {
            const len = el.value.length;
            el.setSelectionRange(len, len);
        }
    }

    return { showLoading, hideLoading, focus, setCaretEnd };
})();

// clipboard copy helper
window.bzCopy = function (text) {
    if (navigator.clipboard && navigator.clipboard.writeText) {
        return navigator.clipboard.writeText(text).then(() => true).catch(() => false);
    }
    try {
        const ta = document.createElement('textarea');
        ta.value = text; ta.style.position = 'fixed'; ta.style.opacity = '0';
        document.body.appendChild(ta); ta.select();
        const ok = document.execCommand('copy');
        document.body.removeChild(ta);
        return Promise.resolve(ok);
    } catch (_) { return Promise.resolve(false); }
};

// download text content as a file (UTF-8 with BOM so Excel reads Persian)
window.bzDownload = function (filename, content, mime) {
    try {
        const bom = '\uFEFF';
        const blob = new Blob([bom + content], { type: (mime || 'text/csv') + ';charset=utf-8;' });
        const url = URL.createObjectURL(blob);
        const a = document.createElement('a');
        a.href = url; a.download = filename || 'download.csv';
        document.body.appendChild(a); a.click();
        document.body.removeChild(a);
        setTimeout(() => URL.revokeObjectURL(url), 1000);
        return true;
    } catch (e) { console.error('bzDownload failed', e); return false; }
};

// Swiper init helper (Swiper loaded from CDN in the host page).
// Retries until both the Swiper library and the target element are ready,
// so the very first page load (before the CDN script finishes) still works.
window.bzSwiper = {
    init: function (selector, options, _tries) {
        _tries = _tries || 0;
        var el = document.querySelector(selector);
        if (typeof Swiper === 'undefined' || !el) {
            if (_tries < 40) { // ~ up to 4s
                setTimeout(function () { window.bzSwiper.init(selector, options, _tries + 1); }, 100);
            } else {
                console.warn('bzSwiper: Swiper or element not ready', selector);
            }
            return;
        }
        // avoid double-initialising the same element
        if (el.swiper) { return el.swiper; }
        try { return new Swiper(selector, options || {}); }
        catch (e) { console.error('bzSwiper.init', e); }
    }
};

// focus an element by id (used by bzOtpInput)
window.bzFocusById = function (id) {
    const el = document.getElementById(id);
    if (el) { el.focus(); if (el.select) el.select(); }
};

// returns true if the OS prefers a dark color scheme
window.bzPrefersDark = function () {
    return window.matchMedia && window.matchMedia('(prefers-color-scheme: dark)').matches;
};

// Theme persistence (used by the demo layouts to remember the chosen theme)
window.bzThemeStore = {
    save: function (kind, appearance, direction) {
        try {
            if (kind != null) localStorage.setItem('bz-theme-kind', kind);
            if (appearance != null) localStorage.setItem('bz-theme-appearance', appearance);
            if (direction != null) localStorage.setItem('bz-theme-direction', direction);
        } catch (e) { }
    },
    load: function () {
        try {
            return {
                kind: localStorage.getItem('bz-theme-kind'),
                appearance: localStorage.getItem('bz-theme-appearance'),
                direction: localStorage.getItem('bz-theme-direction')
            };
        } catch (e) { return { kind: null, appearance: null, direction: null }; }
    }
};

// Extended carousel init: optionally builds a linked thumbnails swiper first,
// then injects its live instance into the main swiper options (kept entirely
// in JS because Swiper instances can't round-trip through .NET interop).
window.bzSwiper.initEx = function (selector, options, thumbSelector, thumbOptions) {
    function build(_tries) {
        _tries = _tries || 0;
        var el = document.querySelector(selector);
        if (typeof Swiper === 'undefined' || !el) {
            if (_tries < 40) { setTimeout(function () { build(_tries + 1); }, 100); }
            return;
        }
        if (el.swiper) { return; }
        try {
            options = options || {};
            if (thumbSelector) {
                var tEl = document.querySelector(thumbSelector);
                if (tEl) {
                    var thumbs = tEl.swiper || new Swiper(thumbSelector, thumbOptions || {});
                    options.thumbs = { swiper: thumbs };
                }
            }
            new Swiper(selector, options);
        } catch (e) { console.error('bzSwiper.initEx', e); }
    }
    build(0);
};

// Reads the chosen image from a file <input>, downscales it on a canvas and
// returns a compact JPEG data-URL. Keeps avatars tiny and avoids large
// SignalR render diffs (which can silently break server-side file uploads).
window.bzReadResizedImage = function (inputId, maxDim) {
    return new Promise(function (resolve) {
        var el = document.getElementById(inputId);
        if (!el || !el.files || !el.files[0]) { resolve(""); return; }
        var file = el.files[0];
        var reader = new FileReader();
        reader.onload = function (e) {
            var img = new Image();
            img.onload = function () {
                var w = img.width, h = img.height;
                var scale = Math.min(1, (maxDim || 256) / Math.max(w, h));
                var cw = Math.max(1, Math.round(w * scale));
                var ch = Math.max(1, Math.round(h * scale));
                var canvas = document.createElement('canvas');
                canvas.width = cw; canvas.height = ch;
                canvas.getContext('2d').drawImage(img, 0, 0, cw, ch);
                try { resolve(canvas.toDataURL('image/jpeg', 0.85)); }
                catch (err) { resolve(e.target.result); }
            };
            img.onerror = function () { resolve(""); };
            img.src = e.target.result;
        };
        reader.onerror = function () { resolve(""); };
        reader.readAsDataURL(file);
    });
};

window.bzOverlay = {
    shouldOpenUp: function (id, estimatedHeight) {
        var el = document.getElementById(id);
        if (!el) return false;
        var rect = el.getBoundingClientRect();
        var needed = estimatedHeight || 320;
        return window.innerHeight - rect.bottom < needed && rect.top > window.innerHeight - rect.bottom;
    }
};
