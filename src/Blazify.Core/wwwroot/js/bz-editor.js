// bz-editor.js — thin wrapper around CKEditor 5 (loaded from CDN in the host page).
// Persian UI + RTL content by default.
window.bzEditor = (function () {
    const map = new WeakMap();

    async function create(el, dotnet, initial, readOnly) {
        if (!el || typeof ClassicEditor === 'undefined') {
            console.warn('CKEditor not loaded — add the CDN script to your host page.');
            return;
        }
        try {
            const editor = await ClassicEditor.create(el, {
                language: { ui: 'fa', content: 'fa' }
            });
            editor.setData(initial || '');
            if (readOnly) editor.ebzleReadOnlyMode('bz');

            editor.model.document.on('change:data', () => {
                dotnet.invokeMethodAsync('OnChanged', editor.getData());
            });
            map.set(el, editor);
        } catch (e) {
            console.error('bzEditor.create failed', e);
        }
    }

    function setData(el, html) {
        const ed = map.get(el);
        if (ed && ed.getData() !== html) ed.setData(html || '');
    }

    function setReadOnly(el, ro) {
        const ed = map.get(el);
        if (!ed) return;
        if (ro) ed.ebzleReadOnlyMode('bz'); else ed.disableReadOnlyMode('bz');
    }

    function destroy(el) {
        const ed = map.get(el);
        if (ed) { ed.destroy().catch(() => {}); map.delete(el); }
    }

    return { create, setData, setReadOnly, destroy };
})();
