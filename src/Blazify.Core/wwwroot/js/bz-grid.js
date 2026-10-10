// bz-grid.js — column resizing via drag on header resizer handles
window.bzGridResize = {
    init: function (gridId) {
        const root = document.getElementById(gridId);
        if (!root) return;
        const ths = root.querySelectorAll('thead th');
        ths.forEach(th => {
            const handle = th.querySelector('.bz-grid__resizer');
            if (!handle || handle.dataset.bound) return;
            handle.dataset.bound = '1';
            let startX = 0, startW = 0;
            const down = e => {
                startX = e.clientX; startW = th.offsetWidth;
                const move = ev => {
                    // RTL: dragging start handle; width grows as pointer moves outward
                    const delta = startX - ev.clientX;
                    th.style.width = Math.max(60, startW + delta) + 'px';
                };
                const up = () => { document.removeEventListener('pointermove', move); document.removeEventListener('pointerup', up); };
                document.addEventListener('pointermove', move);
                document.addEventListener('pointerup', up);
                e.preventDefault(); e.stopPropagation();
            };
            handle.addEventListener('pointerdown', down);
        });
    }
};

// bzGridAdvance: print just the grid
window.bzAdvGrid = window.bzAdvGrid || {};
window.bzAdvGrid.print = function (gridId) {
    var el = document.getElementById(gridId);
    if (!el) { window.print(); return; }
    var w = window.open('', '_blank');
    if (!w) return;
    w.document.write('<html dir="rtl"><head><title>Print</title>');
    w.document.write('<style>table{border-collapse:collapse;width:100%;font-family:sans-serif}th,td{border:1px solid #ccc;padding:6px 10px;text-align:right;font-size:12px}thead th{background:#f1f5f9}</style>');
    w.document.write('</head><body>');
    var table = el.querySelector('table');
    w.document.write(table ? table.outerHTML : el.outerHTML);
    w.document.write('</body></html>');
    w.document.close();
    w.focus();
    setTimeout(function(){ w.print(); w.close(); }, 250);
};

// bzGridAdvance: clipboard copy / paste (Excel-like)
window.bzAdvGrid = window.bzAdvGrid || {};
window.bzAdvGrid.copy = async function (text) {
    try {
        if (navigator.clipboard && window.isSecureContext) { await navigator.clipboard.writeText(text); return true; }
    } catch (e) { /* fall through */ }
    try {
        var ta = document.createElement('textarea');
        ta.value = text; ta.style.position = 'fixed'; ta.style.opacity = '0';
        document.body.appendChild(ta); ta.select();
        document.execCommand('copy'); document.body.removeChild(ta); return true;
    } catch (e) { return false; }
};
window.bzAdvGrid.paste = async function () {
    try {
        if (navigator.clipboard && navigator.clipboard.readText) { return await navigator.clipboard.readText(); }
    } catch (e) { /* fall through */ }
    return ''; // clipboard read not permitted
};

// bzGridAdvance: Excel / PDF export + Excel import.
// These use optional CDN libs loaded in the host page:
//   SheetJS (XLSX)  -> https://cdn.sheetjs.com/xlsx-0.20.x/package/dist/xlsx.full.min.js
//   jsPDF + autoTable for PDF.
// If a lib is missing, a sensible fallback is used (CSV download / print).
window.bzAdvGrid = window.bzAdvGrid || {};

window.bzAdvGrid.exportExcel = function (fileName, payload) {
    try {
        if (typeof XLSX !== 'undefined') {
            var aoa = [payload.headers].concat(payload.rows);
            var ws = XLSX.utils.aoa_to_sheet(aoa);
            var wb = XLSX.utils.book_new();
            XLSX.utils.book_append_sheet(wb, ws, 'Sheet1');
            XLSX.writeFile(wb, fileName + '.xlsx');
            return;
        }
    } catch (e) { console.error('exportExcel', e); }
    // fallback: CSV
    var csv = [payload.headers].concat(payload.rows)
        .map(function (r) { return r.map(function (c) {
            c = (c == null ? '' : '' + c);
            return /[",\n]/.test(c) ? '"' + c.replace(/"/g, '""') + '"' : c;
        }).join(','); }).join('\n');
    window.bzDownload(fileName + '.csv', '\uFEFF' + csv, 'text/csv');
};

window.bzAdvGrid.exportPdf = function (fileName, payload, gridId) {
    try {
        var JsPdf = (window.jspdf && window.jspdf.jsPDF) || window.jsPDF;
        if (JsPdf) {
            var doc = new JsPdf({ orientation: 'landscape' });
            if (typeof doc.autoTable === 'function') {
                doc.autoTable({ head: [payload.headers], body: payload.rows, styles: { font: 'helvetica', halign: 'right' } });
            } else {
                doc.text(payload.title || 'export', 14, 16);
                var y = 24;
                payload.rows.slice(0, 40).forEach(function (r) { doc.text(r.join('  |  '), 14, y); y += 7; });
            }
            doc.save(fileName + '.pdf');
            return;
        }
    } catch (e) { console.error('exportPdf', e); }
    // fallback: print the grid
    window.bzAdvGrid.print(gridId);
};

window.bzAdvGrid.parseExcel = function (base64) {
    try {
        if (typeof XLSX === 'undefined') return '[]';
        var data = atob(base64);
        var arr = new Uint8Array(data.length);
        for (var i = 0; i < data.length; i++) arr[i] = data.charCodeAt(i);
        var wb = XLSX.read(arr, { type: 'array' });
        var ws = wb.Sheets[wb.SheetNames[0]];
        var json = XLSX.utils.sheet_to_json(ws, { defval: '', raw: false });
        return JSON.stringify(json);
    } catch (e) { console.error('parseExcel', e); return '[]'; }
};

// bzGridAdvance: state persistence (localStorage)
window.bzAdvGrid = window.bzAdvGrid || {};
window.bzAdvGrid.saveState = function (key, json) {
    try { localStorage.setItem(key, json); } catch (e) { /* ignore */ }
};
window.bzAdvGrid.loadState = function (key) {
    try { return localStorage.getItem(key); } catch (e) { return null; }
};
