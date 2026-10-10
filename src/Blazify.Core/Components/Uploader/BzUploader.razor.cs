using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Blazify.Core.Base;
using BzCore.Enums;

namespace BzCore.Components.Uploader;

// BzUploader — file picker with several looks (dropzone, button, avatar, gallery).
// It validates size/count, builds base64 preview thumbnails for images, keeps the
// chosen files in _files, and raises OnFilesChanged. It does NOT upload anything —
// you read each BzUploadFile.File and send it wherever you need.
public partial class BzUploader : BzComponentBase
{
    private readonly List<BzUploadFile> _files = new();
    private string? _error;

    /// <summary>Visual style (Dropzone, Button, Avatar, Gallery).</summary>
    [Parameter] public BzUploaderVariant Variant { get; set; } = BzUploaderVariant.Dropzone;
    /// <summary>Allow selecting more than one file.</summary>
    [Parameter] public bool Multiple { get; set; } = true;
    /// <summary>HTML accept filter (e.g. "image/*,.pdf").</summary>
    [Parameter] public string? Accept { get; set; }
    /// <summary>Maximum size per file, in megabytes.</summary>
    [Parameter] public double MaxSizeMB { get; set; } = 10;
    /// <summary>Maximum number of files.</summary>
    [Parameter] public int MaxFiles { get; set; } = 20;

    [Parameter] public string Title { get; set; } = "فایل ها را اینجا رها کنید";
    [Parameter] public string Hint { get; set; } = "یا برای انتخاب کلیک کنید";
    [Parameter] public string ButtonText { get; set; } = "انتخاب فایل";

    /// <summary>Fires whenever the selected files change.</summary>
    [Parameter] public EventCallback<IReadOnlyList<BzUploadFile>> OnFilesChanged { get; set; }

    /// <summary>The currently selected files.</summary>
    public IReadOnlyList<BzUploadFile> Files => _files;

    private long MaxBytes => (long)(MaxSizeMB * 1024 * 1024);
    private string RootClass => RootCss("bz-up").Add($"bz-up--{Variant.ToString().ToLowerInvariant()}").Build();

    private async Task OnChange(InputFileChangeEventArgs e)
    {
        _error = null;
        var incoming = Multiple ? e.GetMultipleFiles(MaxFiles) : new[] { e.File };

        foreach (var file in incoming)
        {
            if (!Multiple) _files.Clear();                       // single mode: keep only the latest
            if (_files.Count >= MaxFiles) { _error = $"حداکثر {MaxFiles} فایل مجاز است."; break; }
            if (file.Size > MaxBytes) { _error = $"{file.Name} بزرگ تر از {MaxSizeMB} مگابایت است."; continue; }

            var item = new BzUploadFile
            {
                Name = file.Name, Size = file.Size, ContentType = file.ContentType, File = file
            };

            if (item.IsImage) await BuildPreview(item, file);
            _files.Add(item);
        }

        await Notify();
    }

    // Read the image once and store it as a base64 data-URL for the thumbnail.
    private static async Task BuildPreview(BzUploadFile item, IBrowserFile file)
    {
        try
        {
            using var stream = file.OpenReadStream(item.Size);
            using var ms = new MemoryStream();
            await stream.CopyToAsync(ms);
            item.PreviewUrl = $"data:{file.ContentType};base64,{Convert.ToBase64String(ms.ToArray())}";
        }
        catch { /* preview is best-effort */ }
    }

    private async Task Remove(BzUploadFile file)
    {
        _files.Remove(file);
        _error = null;
        await Notify();
    }

    /// <summary>Remove all selected files.</summary>
    public async Task Clear() { _files.Clear(); await Notify(); }

    private async Task Notify()
    {
        if (OnFilesChanged.HasDelegate) await OnFilesChanged.InvokeAsync(_files);
        StateHasChanged();
    }
}
