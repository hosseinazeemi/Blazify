using Microsoft.AspNetCore.Components.Forms;

namespace BzCore.Components.Uploader;

/// <summary>A file picked in a <see cref="BzUploader"/>, plus display helpers.</summary>
public sealed class BzUploadFile
{
    /// <summary>Original file name.</summary>
    public string Name { get; init; } = "";
    /// <summary>File size in bytes.</summary>
    public long Size { get; init; }
    /// <summary>MIME type reported by the browser.</summary>
    public string ContentType { get; init; } = "";
    /// <summary>The underlying browser file (use this to read/upload the content).</summary>
    public IBrowserFile File { get; init; } = default!;

    /// <summary>Data-URL thumbnail for images (null for non-images).</summary>
    public string? PreviewUrl { get; set; }

    /// <summary>True when the content type is an image.</summary>
    public bool IsImage => ContentType.StartsWith("image/", StringComparison.OrdinalIgnoreCase);

    /// <summary>Human-friendly size, e.g. "1.4 MB".</summary>
    public string SizeText => Size switch
    {
        < 1024 => $"{Size} B",
        < 1024 * 1024 => $"{Size / 1024.0:0.#} KB",
        _ => $"{Size / (1024.0 * 1024):0.#} MB"
    };
}
