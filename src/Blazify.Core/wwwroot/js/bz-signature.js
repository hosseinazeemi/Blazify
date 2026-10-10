// bz-signature.js — lightweight canvas signature pad (pointer events)
window.bzSignature = (function () {
    const state = new WeakMap();

    function setup(canvas) {
        const ctx = canvas.getContext('2d');
        const rect = canvas.getBoundingClientRect();
        const dpr = window.devicePixelRatio || 1;
        canvas.width = Math.max(1, rect.width * dpr);
        canvas.height = Math.max(1, rect.height * dpr);
        ctx.scale(dpr, dpr);
        ctx.lineWidth = 2.4;
        ctx.lineCap = 'round';
        ctx.lineJoin = 'round';
        const cs = getComputedStyle(canvas);
        ctx.strokeStyle = cs.color || '#111827';
        return ctx;
    }

    function init(canvas, dotnet) {
        if (!canvas) return;
        const ctx = setup(canvas);
        const st = { ctx, drawing: false, dirty: false, dotnet, last: null };
        state.set(canvas, st);

        const pos = e => {
            const r = canvas.getBoundingClientRect();
            return { x: e.clientX - r.left, y: e.clientY - r.top };
        };
        const start = e => { st.drawing = true; st.last = pos(e); e.preventDefault(); };
        const move = e => {
            if (!st.drawing) return;
            const p = pos(e);
            ctx.beginPath(); ctx.moveTo(st.last.x, st.last.y); ctx.lineTo(p.x, p.y); ctx.stroke();
            st.last = p; st.dirty = true; e.preventDefault();
        };
        const end = () => {
            if (!st.drawing) return;
            st.drawing = false;
            if (st.dirty && st.dotnet) st.dotnet.invokeMethodAsync('OnChanged', canvas.toDataURL('image/png'));
        };

        canvas.addEventListener('pointerdown', start);
        canvas.addEventListener('pointermove', move);
        window.addEventListener('pointerup', end);
        st.handlers = { start, move, end };
    }

    function clear(canvas) {
        const st = state.get(canvas);
        if (!st) return;
        st.ctx.clearRect(0, 0, canvas.width, canvas.height);
        st.dirty = false;
        if (st.dotnet) st.dotnet.invokeMethodAsync('OnChanged', '');
    }

    function toDataURL(canvas) { return canvas ? canvas.toDataURL('image/png') : ''; }

    function destroy(canvas) {
        const st = state.get(canvas);
        if (st && st.handlers) {
            canvas.removeEventListener('pointerdown', st.handlers.start);
            canvas.removeEventListener('pointermove', st.handlers.move);
            window.removeEventListener('pointerup', st.handlers.end);
        }
        state.delete(canvas);
    }

    return { init, clear, toDataURL, destroy };
})();

// download the canvas as a PNG file
window.bzSignatureDownload = function (canvas, name) {
    try {
        const a = document.createElement('a');
        a.href = canvas.toDataURL('image/png');
        a.download = name || 'signature.png';
        document.body.appendChild(a); a.click(); document.body.removeChild(a);
    } catch (e) { console.error('bzSignatureDownload', e); }
};
