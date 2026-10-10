// bz-dialog.js — body scroll lock that supports stacked dialogs.
window.bzDialog = (function () {
    let count = 0;
    let prevOverflow = '';

    function lock() {
        if (count === 0) {
            prevOverflow = document.body.style.overflow;
            document.body.style.overflow = 'hidden';
        }
        count++;
    }

    function unlock() {
        count = Math.max(0, count - 1);
        if (count === 0) document.body.style.overflow = prevOverflow;
    }

    return { lock, unlock };
})();
