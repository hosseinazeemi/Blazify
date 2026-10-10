using System.Globalization;
using System.Text;
using Blazify.Core.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.JSInterop;

namespace BzCore.Components.Document;

/// <summary>
/// A printable document: a true-to-size paper preview on screen, and print output
/// that matches it exactly.
/// </summary>
/// <remarks>
/// <para><b>Why the browser makes the PDF.</b> Persian needs the bidi algorithm (UAX #9)
/// plus Arabic contextual shaping — every letter has isolated, initial, medial and
/// final forms, and lam-alef ligates. A from-scratch PDF writer has to implement all
/// of that, plus TrueType subsetting, before a single word renders correctly. The
/// browser already does it, on every page, every day. So this component composes the
/// document and hands it to the print engine, which is also the only "pure HTML + C#"
/// route that gets Persian right.</para>
/// <para><b>Do not reach for jsPDF or html2pdf.</b> They rasterise or re-typeset without
/// shaping, which breaks Persian into disconnected letters. They are a step backwards
/// here, not forwards.</para>
/// <para><b>Getting an actual file.</b> The print dialog's "Save as PDF" destination
/// writes a real, selectable, searchable PDF. For unattended server-side generation
/// you need a library or headless browser — see the readme.</para>
/// </remarks>
public partial class BzDocument : BzComponentBase
{
    [Inject] private IJSRuntime JS { get; set; } = default!;

    // ---------------- content ----------------

    /// <summary>The document body.</summary>
    [Parameter] public RenderFragment? ChildContent { get; set; }

    /// <summary>
    /// Repeated at the top of every printed page. See the readme for the browser
    /// caveat on running headers.
    /// </summary>
    [Parameter] public RenderFragment? Header { get; set; }

    /// <summary>Repeated at the bottom of every printed page.</summary>
    [Parameter] public RenderFragment? Footer { get; set; }

    /// <summary>Rendered as a full page before the body, on its own sheet.</summary>
    [Parameter] public RenderFragment? Cover { get; set; }

    // ---------------- page setup ----------------

    /// <summary>Paper size.</summary>
    [Parameter] public BzPageSize Size { get; set; } = BzPageSize.A4;

    /// <summary>Paper orientation.</summary>
    [Parameter] public BzPageOrientation Orientation { get; set; } = BzPageOrientation.Portrait;

    /// <summary>Page margin. Any CSS length, or four values like "20mm 15mm".</summary>
    [Parameter] public string Margin { get; set; } = "18mm";

    /// <summary>Width when <see cref="Size"/> is Custom, in millimetres.</summary>
    [Parameter] public double CustomWidth { get; set; } = 210;

    /// <summary>Height when <see cref="Size"/> is Custom, in millimetres.</summary>
    [Parameter] public double CustomHeight { get; set; } = 297;

    /// <summary>Reading direction.</summary>
    [Parameter] public BzDocDirection Direction { get; set; } = BzDocDirection.Auto;

    // ---------------- appearance ----------------

    /// <summary>Diagonal watermark text. Null hides it.</summary>
    [Parameter] public string? Watermark { get; set; }

    /// <summary>Watermark opacity, 0 to 1.</summary>
    [Parameter] public double WatermarkOpacity { get; set; } = 0.08;

    /// <summary>Base font size of the document body.</summary>
    [Parameter] public string FontSize { get; set; } = "12pt";

    /// <summary>Base line height.</summary>
    [Parameter] public string LineHeight { get; set; } = "1.8";

    /// <summary>
    /// Keeps background colours and images in the printed output. Browsers drop them
    /// by default, which turns a designed invoice into plain text.
    /// </summary>
    [Parameter] public bool PrintBackgrounds { get; set; } = true;

    // ---------------- toolbar ----------------

    /// <summary>Which toolbar buttons to show.</summary>
    [Parameter] public BzDocTools Tools { get; set; } = BzDocTools.Basic;

    /// <summary>Suggested file name, shown as a hint in the toolbar.</summary>
    [Parameter] public string? FileName { get; set; }

    /// <summary>Margin presets offered by the toolbar.</summary>
    [Parameter] public IReadOnlyList<BzDocMargin>? MarginPresets { get; set; }

    /// <summary>Raised just before the print dialog opens.</summary>
    [Parameter] public EventCallback OnBeforePrint { get; set; }

    // ---------------- state ----------------

    private int _zoom = 100;
    private BzPageOrientation _orientation;
    private BzPageSize _size;
    private string _margin = "18mm";
    private bool _showWatermark = true;
    private bool _menuOpen;
    private string? _menu;

    private static bool IsFaCulture =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName is "fa" or "ar" or "he";

    private bool Fa => IsFaCulture;

    private bool Rtl => Direction switch
    {
        BzDocDirection.Rtl => true,
        BzDocDirection.Ltr => false,
        _ => IsFaCulture,
    };

    private string DirAttr => Rtl ? "rtl" : "ltr";

    private bool Has(BzDocTools tool) => (Tools & tool) == tool;

    private bool HasToolbar => Tools != BzDocTools.None;

    private IReadOnlyList<BzDocMargin> Margins => MarginPresets ?? DefaultMargins;

    private IReadOnlyList<BzDocMargin> DefaultMargins => new[]
    {
        new BzDocMargin(Fa ? "بدون حاشیه" : "None", "0"),
        new BzDocMargin(Fa ? "باریک" : "Narrow", "10mm"),
        new BzDocMargin(Fa ? "معمولی" : "Normal", "18mm"),
        new BzDocMargin(Fa ? "پهن" : "Wide", "25mm"),
    };

    private static readonly BzPageSize[] SizeOptions =
    {
        BzPageSize.A3, BzPageSize.A4, BzPageSize.A5,
        BzPageSize.Letter, BzPageSize.Legal, BzPageSize.Tabloid,
    };

    protected override void OnInitialized()
    {
        base.OnInitialized();
        _orientation = Orientation;
        _size = Size;
        _margin = Margin;
    }

    protected override void OnParametersSet()
    {
        // Let the host drive these when it re-renders with new values.
        if (Orientation != _orientation && !_userChangedOrientation) _orientation = Orientation;
        if (Size != _size && !_userChangedSize) _size = Size;
        if (Margin != _margin && !_userChangedMargin) _margin = Margin;
    }

    private bool _userChangedOrientation;
    private bool _userChangedSize;
    private bool _userChangedMargin;

    // ---------------- geometry ----------------

    /// <summary>Paper width in millimetres, after orientation.</summary>
    private double PaperW
    {
        get
        {
            var (w, h) = _size == BzPageSize.Custom
                ? (CustomWidth, CustomHeight)
                : BzPageMetrics.Portrait(_size);
            return _orientation == BzPageOrientation.Landscape ? h : w;
        }
    }

    /// <summary>Paper height in millimetres, after orientation.</summary>
    private double PaperH
    {
        get
        {
            var (w, h) = _size == BzPageSize.Custom
                ? (CustomWidth, CustomHeight)
                : BzPageMetrics.Portrait(_size);
            return _orientation == BzPageOrientation.Landscape ? w : h;
        }
    }

    private string Inv(double v) => v.ToString("0.##", CultureInfo.InvariantCulture);

    /// <summary>
    /// Builds the whole stylesheet from the parameters and renders it inline.
    /// Doing this in C# rather than JS keeps the component pure — and the same
    /// numbers drive both the preview and the @page rule, so they cannot drift.
    /// </summary>
    private MarkupString PageStyle
    {
        get
        {
            var sb = new StringBuilder();
            var scope = $"#{Id}";

            var sizeCss = _size == BzPageSize.Custom
                ? $"{Inv(PaperW)}mm {Inv(PaperH)}mm"
                : $"{BzPageMetrics.CssKeyword(_size)} {_orientation.ToString().ToLowerInvariant()}";

            // The @page rule is global by nature: CSS has no way to scope it, so a
            // page hosting two BzDocuments with different setups will use the last
            // one rendered. That is a CSS limitation, not an oversight.
            sb.Append("@media print{");
            sb.Append("@page{size:").Append(sizeCss).Append(";margin:").Append(_margin).Append(";}");
            sb.Append("}");

            // Preview paper, sized in real millimetres and scaled by the zoom.
            sb.Append(scope).Append(" .bz-doc__paper{");
            sb.Append("width:").Append(Inv(PaperW)).Append("mm;");
            sb.Append("min-height:").Append(Inv(PaperH)).Append("mm;");
            sb.Append("padding:").Append(_margin).Append(';');
            sb.Append("font-size:").Append(FontSize).Append(';');
            sb.Append("line-height:").Append(LineHeight).Append(';');
            sb.Append('}');

            sb.Append(scope).Append(" .bz-doc__stage{");
            sb.Append("zoom:").Append(Inv(_zoom / 100.0)).Append(';');
            sb.Append('}');

            if (!string.IsNullOrWhiteSpace(Watermark) && _showWatermark)
            {
                sb.Append(scope).Append(" .bz-doc__watermark{opacity:")
                  .Append(Inv(WatermarkOpacity)).Append(";}");
            }

            return new MarkupString($"<style>{sb}</style>");
        }
    }

    // ---------------- toolbar actions ----------------

    private void Zoom(int delta) => _zoom = Math.Clamp(_zoom + delta, 40, 200);

    private void ZoomReset() => _zoom = 100;

    private void ToggleMenu(string name) => _menu = _menu == name ? null : name;

    private void SetOrientation(BzPageOrientation o)
    {
        _orientation = o;
        _userChangedOrientation = true;
        _menu = null;
    }

    private void SetSize(BzPageSize s)
    {
        _size = s;
        _userChangedSize = true;
        _menu = null;
    }

    private void SetMargin(string m)
    {
        _margin = m;
        _userChangedMargin = true;
        _menu = null;
    }

    private void ToggleWatermark() => _showWatermark = !_showWatermark;

    /// <summary>
    /// Opens the print dialog. From there the user can pick a printer or
    /// "Save as PDF" to write a real, searchable PDF file.
    /// </summary>
    public async Task PrintAsync()
    {
        if (OnBeforePrint.HasDelegate) await OnBeforePrint.InvokeAsync();
        try { await JS.InvokeVoidAsync("window.print"); }
        catch (JSException) { /* no JS host */ }
        catch (InvalidOperationException) { /* prerendering */ }
    }

    private string SizeLabel(BzPageSize s) => s switch
    {
        BzPageSize.Letter => "Letter",
        BzPageSize.Legal => "Legal",
        BzPageSize.Tabloid => "Tabloid",
        BzPageSize.Custom => Fa ? "دلخواه" : "Custom",
        _ => s.ToString(),
    };
}
