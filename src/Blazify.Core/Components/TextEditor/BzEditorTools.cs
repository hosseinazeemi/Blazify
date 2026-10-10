namespace BzCore.Components.TextEditor;

/// <summary>
/// Every toolbar tool, as flags — combine them to build exactly the toolbar you want.
/// </summary>
/// <example>
/// <code>
/// @* just the essentials *@
/// &lt;BzTextEditor Tools="BzEditorTools.Basic" /&gt;
///
/// @* a preset, minus one tool, plus another *@
/// &lt;BzTextEditor Tools="BzEditorTools.Standard &amp; ~BzEditorTools.Underline | BzEditorTools.Table" /&gt;
///
/// @* everything *@
/// &lt;BzTextEditor Tools="BzEditorTools.Full" /&gt;
/// </code>
/// </example>
[Flags]
public enum BzEditorTools : long
{
    None = 0,

    // ---- history ----
    Undo = 1L << 0,
    Redo = 1L << 1,

    // ---- inline text ----
    Bold = 1L << 2,
    Italic = 1L << 3,
    Underline = 1L << 4,
    Strike = 1L << 5,
    Subscript = 1L << 6,
    Superscript = 1L << 7,

    // ---- block / typography ----
    Heading = 1L << 8,
    FontFamily = 1L << 9,
    FontSize = 1L << 10,
    LineHeight = 1L << 11,

    // ---- colour ----
    TextColor = 1L << 12,
    Highlight = 1L << 13,

    // ---- alignment ----
    AlignStart = 1L << 14,
    AlignCenter = 1L << 15,
    AlignEnd = 1L << 16,
    AlignJustify = 1L << 17,

    // ---- direction (the reason this editor exists) ----
    DirLtr = 1L << 18,
    DirRtl = 1L << 19,

    // ---- lists ----
    BulletList = 1L << 20,
    NumberList = 1L << 21,
    Indent = 1L << 22,
    Outdent = 1L << 23,

    // ---- blocks ----
    Quote = 1L << 24,
    CodeBlock = 1L << 25,
    HorizontalRule = 1L << 26,

    // ---- insert ----
    Link = 1L << 27,
    Image = 1L << 28,
    Table = 1L << 29,

    // ---- utility ----
    ClearFormat = 1L << 30,
    Source = 1L << 31,
    Fullscreen = 1L << 32,
    Print = 1L << 33,

    // ================= groups =================

    /// <summary>Undo + Redo.</summary>
    History = Undo | Redo,

    /// <summary>Bold, italic, underline, strikethrough.</summary>
    Inline = Bold | Italic | Underline | Strike,

    /// <summary>Subscript + superscript.</summary>
    Script = Subscript | Superscript,

    /// <summary>Font family, size and line height.</summary>
    Typography = FontFamily | FontSize | LineHeight,

    /// <summary>Text colour + highlight.</summary>
    Colors = TextColor | Highlight,

    /// <summary>All four alignments.</summary>
    Alignment = AlignStart | AlignCenter | AlignEnd | AlignJustify,

    /// <summary>LTR + RTL block direction.</summary>
    Direction = DirLtr | DirRtl,

    /// <summary>Bullets, numbers, indent, outdent.</summary>
    Lists = BulletList | NumberList | Indent | Outdent,

    /// <summary>Quote, code block, horizontal rule.</summary>
    Blocks = Quote | CodeBlock | HorizontalRule,

    /// <summary>Link, image, table.</summary>
    Insert = Link | Image | Table,

    /// <summary>Source view, fullscreen, print.</summary>
    View = Source | Fullscreen | Print,

    // ================= presets =================

    /// <summary>A comment box: inline formatting, lists and links.</summary>
    Basic = History | Inline | BulletList | NumberList | Link | ClearFormat,

    /// <summary>A good default for articles and descriptions.</summary>
    Standard = History | Inline | Heading | Colors | Alignment | Direction
             | Lists | Quote | Link | Image | ClearFormat,

    /// <summary>Everything.</summary>
    Full = History | Inline | Script | Heading | Typography | Colors | Alignment
         | Direction | Lists | Blocks | Insert | ClearFormat | View,
}
