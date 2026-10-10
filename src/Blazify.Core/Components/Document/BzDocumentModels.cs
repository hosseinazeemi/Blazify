namespace BzCore.Components.Document;

/// <summary>Paper size. The names match the CSS <c>size</c> keywords, so print output and preview agree.</summary>
public enum BzPageSize
{
    A3,
    A4,
    A5,
    Letter,
    Legal,
    Tabloid,

    /// <summary>Use <see cref="BzDocument.CustomWidth"/> and <see cref="BzDocument.CustomHeight"/>.</summary>
    Custom,
}

/// <summary>Paper orientation.</summary>
public enum BzPageOrientation
{
    Portrait,
    Landscape,
}

/// <summary>Reading direction of the document.</summary>
public enum BzDocDirection
{
    /// <summary>Follow the ambient culture (fa/ar/he become RTL).</summary>
    Auto,
    Ltr,
    Rtl,
}

/// <summary>Toolbar buttons, as flags — combine to build the toolbar you want.</summary>
[Flags]
public enum BzDocTools
{
    None = 0,

    /// <summary>Opens the print dialog, where the user can also choose "Save as PDF".</summary>
    Print = 1 << 0,

    /// <summary>Zoom controls for the on-screen preview only. Never affects print output.</summary>
    Zoom = 1 << 1,

    /// <summary>Switch between portrait and landscape.</summary>
    Orientation = 1 << 2,

    /// <summary>Paper size picker.</summary>
    PageSize = 1 << 3,

    /// <summary>Margin picker.</summary>
    Margins = 1 << 4,

    /// <summary>Toggle the watermark.</summary>
    Watermark = 1 << 5,

    Basic = Print | Zoom,
    All = Print | Zoom | Orientation | PageSize | Margins | Watermark,
}

/// <summary>A named margin preset.</summary>
/// <param name="Label">Shown in the toolbar.</param>
/// <param name="Value">Any CSS length, e.g. "20mm".</param>
public sealed record BzDocMargin(string Label, string Value);

/// <summary>Millimetre dimensions for each paper size, used to draw the preview at true proportions.</summary>
internal static class BzPageMetrics
{
    /// <summary>Returns (width, height) in millimetres for the portrait form of a size.</summary>
    public static (double W, double H) Portrait(BzPageSize size) => size switch
    {
        BzPageSize.A3 => (297, 420),
        BzPageSize.A4 => (210, 297),
        BzPageSize.A5 => (148, 210),
        BzPageSize.Letter => (215.9, 279.4),
        BzPageSize.Legal => (215.9, 355.6),
        BzPageSize.Tabloid => (279.4, 431.8),
        _ => (210, 297),
    };

    /// <summary>The CSS <c>size</c> keyword for a paper size, or null for Custom.</summary>
    public static string? CssKeyword(BzPageSize size) => size switch
    {
        BzPageSize.A3 => "A3",
        BzPageSize.A4 => "A4",
        BzPageSize.A5 => "A5",
        BzPageSize.Letter => "letter",
        BzPageSize.Legal => "legal",
        BzPageSize.Tabloid => "ledger",
        _ => null,
    };
}
