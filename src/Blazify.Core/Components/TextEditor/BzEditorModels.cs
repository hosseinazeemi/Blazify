using Microsoft.AspNetCore.Components.Forms;

namespace BzCore.Components.TextEditor;

/// <summary>Direction of the editing surface.</summary>
public enum BzEditorDirection
{
    /// <summary>Follow the ambient culture (fa/ar/he become RTL).</summary>
    Auto,
    Ltr,
    Rtl,
}

/// <summary>How an image sits in the text.</summary>
public enum BzImageAlign
{
    /// <summary>Inline with the text.</summary>
    None,
    /// <summary>Floated to the start edge, text wraps around.</summary>
    Start,
    /// <summary>Centred on its own line.</summary>
    Center,
    /// <summary>Floated to the end edge, text wraps around.</summary>
    End,
}

/// <summary>A file handed to <see cref="BzTextEditor.OnImageUpload"/>.</summary>
public sealed class BzEditorUpload
{
    /// <summary>The browser file. Read it with <c>OpenReadStream(maxSize)</c>.</summary>
    public required IBrowserFile File { get; init; }

    /// <summary>Original file name.</summary>
    public string Name => File.Name;

    /// <summary>Size in bytes.</summary>
    public long Size => File.Size;

    /// <summary>MIME type reported by the browser. Never trust it alone on the server.</summary>
    public string ContentType => File.ContentType;
}

/// <summary>What the host returns after storing an uploaded image.</summary>
public sealed class BzEditorUploadResult
{
    /// <summary>URL the editor should point the &lt;img&gt; at.</summary>
    public string? Url { get; init; }

    /// <summary>Message to show when <see cref="Url"/> is null.</summary>
    public string? Error { get; init; }

    public static BzEditorUploadResult Ok(string url) => new() { Url = url };
    public static BzEditorUploadResult Fail(string error) => new() { Error = error };
}

/// <summary>
/// Which formats are active at the caret. JS reports this so the toolbar can light up
/// the right buttons; you normally never touch it.
/// </summary>
public sealed class BzEditorState
{
    public bool Bold { get; set; }
    public bool Italic { get; set; }
    public bool Underline { get; set; }
    public bool Strike { get; set; }
    public bool Subscript { get; set; }
    public bool Superscript { get; set; }
    public bool BulletList { get; set; }
    public bool NumberList { get; set; }
    public bool Quote { get; set; }

    /// <summary>Current block tag: p, h1..h6, pre, blockquote.</summary>
    public string? Block { get; set; }

    /// <summary>start | center | end | justify</summary>
    public string? Align { get; set; }

    /// <summary>ltr | rtl for the block under the caret.</summary>
    public string? Dir { get; set; }

    public bool CanUndo { get; set; }
    public bool CanRedo { get; set; }

    /// <summary>Words in the document.</summary>
    public int Words { get; set; }

    /// <summary>Characters in the document.</summary>
    public int Chars { get; set; }
}

/// <summary>Details of the image the caret is currently on, if any.</summary>
public sealed class BzEditorImageInfo
{
    public bool Selected { get; set; }
    public string? Src { get; set; }
    public string? Alt { get; set; }

    /// <summary>Width as authored, e.g. "50%" or "320px".</summary>
    public string? Width { get; set; }

    /// <summary>none | start | center | end</summary>
    public string? Align { get; set; }

    public string? Caption { get; set; }
}

/// <summary>One entry in the font family dropdown.</summary>
/// <param name="Label">Shown to the user.</param>
/// <param name="Stack">CSS font stack applied to the text.</param>
public sealed record BzEditorFont(string Label, string Stack);
