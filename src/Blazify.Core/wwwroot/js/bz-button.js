// bz-button.js — smooth click ripple. Loaded globally as window.BzButton.
window.bzButton = (function () {
    function ripple(el, clientX, clientY) {
        if (!el) return;
        const rect = el.getBoundingClientRect();
        const size = Math.max(rect.width, rect.height);
        const span = document.createElement('span');
        span.className = 'bz-ripple';
        span.style.width = span.style.height = size + 'px';

        const x = (clientX || rect.left + rect.width / 2) - rect.left - size / 2;
        const y = (clientY || rect.top + rect.height / 2) - rect.top - size / 2;
        span.style.left = x + 'px';
        span.style.top = y + 'px';

        el.appendChild(span);
        span.addEventListener('animationend', () => span.remove());
    }

    return { ripple };
})();
