// bz-texteditor.js
// ---------------------------------------------------------------------------
// The smallest JS layer BzTextEditor can get away with.
//
// Why any JS at all: a rich text editor needs contenteditable, and the Selection
// / Range APIs have no C# equivalent. Blazor also cannot own the editable DOM —
// the browser mutates it on every keystroke, which would desync Blazor's diff
// tree and corrupt the content on the next render. So: Blazor renders the
// editable div with NO children and never touches inside it; this file owns it.
//
// Everything that CAN live in C# does: the toolbar, all state, every parameter,
// dialogs, uploads. This file only does what the DOM forces it to.
// ---------------------------------------------------------------------------
window.bzTextEditor = (function () {
    'use strict';

    /** @type {Object<string, Instance>} */
    const instances = {};

    // ---- helpers ----------------------------------------------------------

    function sel() { return window.getSelection(); }

    function inside(root, node) {
        while (node) {
            if (node === root) return true;
            node = node.parentNode;
        }
        return false;
    }

    /** Nearest block-level element containing the caret. */
    function currentBlock(root) {
        const s = sel();
        if (!s || !s.rangeCount) return null;
        let n = s.getRangeAt(0).startContainer;
        if (n.nodeType === 3) n = n.parentNode;
        while (n && n !== root) {
            const d = getComputedStyle(n).display;
            if (d === 'block' || d === 'list-item' || d === 'table-cell') return n;
            n = n.parentNode;
        }
        return root;
    }

    function tagOf(root) {
        const b = currentBlock(root);
        return b && b !== root ? b.tagName.toLowerCase() : 'p';
    }

    function cmd(name, value) {
        try { document.execCommand(name, false, value === undefined ? null : value); }
        catch (e) { /* unsupported command in this browser */ }
    }

    function q(name) {
        try { return document.queryCommandState(name); } catch (e) { return false; }
    }

    function countWords(text) {
        const t = text.replace(/\s+/g, ' ').trim();
        return t ? t.split(' ').length : 0;
    }

    /**
     * Wraps the current selection in a span carrying `style`.
     * execCommand('fontSize') only accepts the legacy 1..7 scale, and there is no
     * command at all for line-height — so these two are done by hand.
     */
    function styleSelection(root, style, value) {
        const s = sel();
        if (!s || !s.rangeCount) return;
        const range = s.getRangeAt(0);
        if (!inside(root, range.commonAncestorContainer)) return;

        if (range.collapsed) {
            // Nothing selected: apply to the whole block, which is what a user
            // means by "make this paragraph bigger".
            const block = currentBlock(root);
            if (block) block.style[style] = value;
            return;
        }

        const span = document.createElement('span');
        span.style[style] = value;
        try {
            range.surroundContents(span);
        } catch (e) {
            // surroundContents throws when the range straddles element boundaries;
            // extract + re-insert handles that case.
            span.appendChild(range.extractContents());
            range.insertNode(span);
        }
        s.removeAllRanges();
        const r = document.createRange();
        r.selectNodeContents(span);
        s.addRange(r);
    }

    // ---- instance ---------------------------------------------------------

    function Instance(id, root, dotnet, options) {
        this.id = id;
        this.root = root;
        this.dotnet = dotnet;
        this.options = options || {};
        this.savedRange = null;
        this.selectedImage = null;
        this.lastHtml = '';
    }

    Instance.prototype.emitHtml = function () {
        const html = this.root.innerHTML;
        if (html === this.lastHtml) return;
        this.lastHtml = html;
        this.dotnet.invokeMethodAsync('OnHtmlChangedAsync', html);
    };

    Instance.prototype.emitState = function () {
        const text = this.root.innerText || '';
        const block = currentBlock(this.root);
        const cs = block ? getComputedStyle(block) : null;

        let align = 'start';
        if (cs) {
            const ta = cs.textAlign;
            if (ta === 'center') align = 'center';
            else if (ta === 'justify') align = 'justify';
            else if (ta === 'right' || ta === 'end') align = cs.direction === 'rtl' ? 'start' : 'end';
            else if (ta === 'left' || ta === 'start') align = cs.direction === 'rtl' ? 'end' : 'start';
        }

        const state = {
            bold: q('bold'),
            italic: q('italic'),
            underline: q('underline'),
            strike: q('strikeThrough'),
            subscript: q('subscript'),
            superscript: q('superscript'),
            bulletList: q('insertUnorderedList'),
            numberList: q('insertOrderedList'),
            quote: tagOf(this.root) === 'blockquote',
            block: tagOf(this.root),
            align: align,
            dir: cs ? cs.direction : 'ltr',
            canUndo: true,
            canRedo: true,
            words: countWords(text),
            chars: text.length,
        };
        this.dotnet.invokeMethodAsync('OnStateChangedAsync', JSON.stringify(state));
    };

    Instance.prototype.save = function () {
        const s = sel();
        if (s && s.rangeCount && inside(this.root, s.getRangeAt(0).commonAncestorContainer))
            this.savedRange = s.getRangeAt(0).cloneRange();
    };

    Instance.prototype.restore = function () {
        if (!this.savedRange) { this.root.focus(); return; }
        // Focus first: focusing contenteditable after restoring a range can collapse
        // the selection in Chromium, making toolbar commands appear to do nothing.
        try { this.root.focus({ preventScroll: true }); }
        catch (_) { this.root.focus(); }
        const s = sel();
        s.removeAllRanges();
        s.addRange(this.savedRange);
    };

    Instance.prototype.selectImage = function (img) {
        if (this.selectedImage) this.selectedImage.classList.remove('bz-te-img--selected');
        this.selectedImage = img;

        if (!img) {
            this.dotnet.invokeMethodAsync('OnImageSelectedAsync', JSON.stringify({ selected: false }));
            return;
        }

        img.classList.add('bz-te-img--selected');
        const fig = img.closest('figure');
        let align = 'none';
        const holder = fig || img;
        if (holder.classList.contains('bz-te-img--start')) align = 'start';
        else if (holder.classList.contains('bz-te-img--center')) align = 'center';
        else if (holder.classList.contains('bz-te-img--end')) align = 'end';

        this.dotnet.invokeMethodAsync('OnImageSelectedAsync', JSON.stringify({
            selected: true,
            src: img.getAttribute('src'),
            alt: img.getAttribute('alt') || '',
            width: img.style.width || '',
            align: align,
            caption: fig ? (fig.querySelector('figcaption')?.textContent || '') : '',
        }));
    };

    // ---- public API -------------------------------------------------------

    return {
        init: function (id, root, dotnet, options) {
            if (!root) return;
            const inst = new Instance(id, root, dotnet, options);
            instances[id] = inst;

            root.innerHTML = options.html || '';
            inst.lastHtml = root.innerHTML;

            // execCommand defaults to wrapping in <font> / <span style>. Ask the
            // browser for semantic tags where it can manage it.
            try { document.execCommand('styleWithCSS', false, false); } catch (e) { }
            try { document.execCommand('defaultParagraphSeparator', false, 'p'); } catch (e) { }

            inst._onInput = function () { inst.emitHtml(); inst.emitState(); };
            inst._onKeyUp = function () { inst.save(); inst.emitState(); };
            inst._onMouseUp = function (e) {
                inst.save();
                inst.selectImage(e.target && e.target.tagName === 'IMG' ? e.target : null);
                inst.emitState();
            };
            inst._onBlur = function () { inst.save(); inst.emitHtml(); };
            inst._onSelectionChange = function () {
                const s = sel();
                if (s && s.rangeCount && inside(root, s.getRangeAt(0).commonAncestorContainer))
                    inst.save();
            };

            inst._onPaste = function (e) {
                if (!options.plainPaste) return;
                e.preventDefault();
                const text = (e.clipboardData || window.clipboardData).getData('text/plain');
                cmd('insertText', text);
            };

            inst._onDrop = function (e) {
                // Files dropped onto the editor go through the C# upload path, so the
                // host can validate and store them like any other upload.
                if (!e.dataTransfer || !e.dataTransfer.files || !e.dataTransfer.files.length) return;
                e.preventDefault();
                inst.save();
                dotnet.invokeMethodAsync('OnFilesDroppedAsync');
            };

            root.addEventListener('input', inst._onInput);
            root.addEventListener('keyup', inst._onKeyUp);
            root.addEventListener('mouseup', inst._onMouseUp);
            root.addEventListener('blur', inst._onBlur);
            root.addEventListener('paste', inst._onPaste);
            root.addEventListener('drop', inst._onDrop);
            document.addEventListener('selectionchange', inst._onSelectionChange);

            inst.emitState();
        },

        /**
         * Kept for API compatibility. Selection is now preserved by
         * @onmousedown:preventDefault on each toolbar button in the markup, which
         * stops focus leaving the editor WITHOUT swallowing the click the way a
         * blanket mousedown handler on the whole toolbar did.
         */
        holdSelection: function (toolbar) { /* no-op */ },

        exec: function (id, command, value) {
            const inst = instances[id];
            if (!inst) return;
            inst.restore();

            // Commands the browser does not provide.
            if (command === 'bzFontSize') styleSelection(inst.root, 'fontSize', value);
            else if (command === 'bzLineHeight') styleSelection(inst.root, 'lineHeight', value);
            else cmd(command, value);

            inst.save();
            inst.emitHtml();
            inst.emitState();
        },

        /** Sets dir on the block under the caret — the piece execCommand gets wrong for Persian. */
        setDirection: function (id, dir) {
            const inst = instances[id];
            if (!inst) return;
            inst.restore();
            const block = currentBlock(inst.root);
            if (block && block !== inst.root) {
                block.setAttribute('dir', dir);
                block.style.textAlign = '';
            } else {
                inst.root.setAttribute('dir', dir);
            }
            inst.emitHtml();
            inst.emitState();
        },

        /** Alignment that respects the block's own direction. */
        setAlign: function (id, align) {
            const inst = instances[id];
            if (!inst) return;
            inst.restore();
            const block = currentBlock(inst.root);
            const target = (block && block !== inst.root) ? block : inst.root;
            target.style.textAlign = align === 'justify' ? 'justify' : align;
            inst.emitHtml();
            inst.emitState();
        },

        insertHtml: function (id, html) {
            const inst = instances[id];
            if (!inst) return;
            inst.restore();
            cmd('insertHTML', html);
            inst.emitHtml();
            inst.emitState();
        },

        getHtml: function (id) {
            const inst = instances[id];
            return inst ? inst.root.innerHTML : '';
        },

        setHtml: function (id, html) {
            const inst = instances[id];
            if (!inst) return;
            inst.root.innerHTML = html || '';
            inst.lastHtml = inst.root.innerHTML;
            inst.emitState();
        },

        focus: function (id) {
            const inst = instances[id];
            if (inst) inst.root.focus();
        },

        saveSelection: function (id) {
            const inst = instances[id];
            if (inst) inst.save();
        },

        /** Applies the image side-panel settings to the selected image. */
        applyImage: function (id, json) {
            const inst = instances[id];
            if (!inst || !inst.selectedImage) return;
            const o = JSON.parse(json);
            const img = inst.selectedImage;

            if (o.alt !== undefined) img.setAttribute('alt', o.alt || '');
            if (o.width) img.style.width = o.width; else img.style.removeProperty('width');

            let holder = img.closest('figure') || img;

            // A caption needs a <figure>; without one, keep the bare <img>.
            if (o.caption) {
                let fig = img.closest('figure');
                if (!fig) {
                    fig = document.createElement('figure');
                    img.parentNode.insertBefore(fig, img);
                    fig.appendChild(img);
                }
                let cap = fig.querySelector('figcaption');
                if (!cap) { cap = document.createElement('figcaption'); fig.appendChild(cap); }
                cap.textContent = o.caption;
                holder = fig;
            } else {
                const fig = img.closest('figure');
                if (fig) {
                    fig.parentNode.insertBefore(img, fig);
                    fig.remove();
                    holder = img;
                }
            }

            holder.classList.remove('bz-te-img--start', 'bz-te-img--center', 'bz-te-img--end');
            if (o.align && o.align !== 'none') holder.classList.add('bz-te-img--' + o.align);

            inst.emitHtml();
        },

        removeImage: function (id) {
            const inst = instances[id];
            if (!inst || !inst.selectedImage) return;
            const fig = inst.selectedImage.closest('figure');
            (fig || inst.selectedImage).remove();
            inst.selectImage(null);
            inst.emitHtml();
            inst.emitState();
        },

        setReadOnly: function (id, readOnly) {
            const inst = instances[id];
            if (inst) inst.root.setAttribute('contenteditable', readOnly ? 'false' : 'true');
        },

        print: function (id) {
            const inst = instances[id];
            if (!inst) return;
            const w = window.open('', '_blank');
            if (!w) return;
            const dir = inst.root.getAttribute('dir') || 'rtl';
            w.document.write(
                '<html dir="' + dir + '"><head><meta charset="utf-8">' +
                '<style>body{font-family:inherit;padding:2rem;line-height:1.9}' +
                'img{max-width:100%}table{border-collapse:collapse}' +
                'td,th{border:1px solid #999;padding:.4rem}</style></head><body>' +
                inst.root.innerHTML + '</body></html>');
            w.document.close();
            w.focus();
            w.print();
        },

        destroy: function (id) {
            const inst = instances[id];
            if (!inst) return;
            inst.root.removeEventListener('input', inst._onInput);
            inst.root.removeEventListener('keyup', inst._onKeyUp);
            inst.root.removeEventListener('mouseup', inst._onMouseUp);
            inst.root.removeEventListener('blur', inst._onBlur);
            inst.root.removeEventListener('paste', inst._onPaste);
            inst.root.removeEventListener('drop', inst._onDrop);
            document.removeEventListener('selectionchange', inst._onSelectionChange);
            delete instances[id];
        },
    };
})();
