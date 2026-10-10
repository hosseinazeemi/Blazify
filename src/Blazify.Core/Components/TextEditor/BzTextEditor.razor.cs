using System.Globalization;
using System.Text.Json;
using Blazify.Core.Base;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Forms;
using Microsoft.JSInterop;

namespace BzCore.Components.TextEditor;

/// <summary>
/// A rich text editor built for Persian first: per-block direction, RTL-aware
/// alignment, and an image panel that understands start/end rather than left/right.
/// </summary>
/// <remarks>
/// <para><b>How it is put together.</b> Blazor renders the toolbar, every dialog and
/// all state. The editable surface is a single div that Blazor renders with no
/// children — so its diff never walks inside — and a small JS file owns that DOM.
/// That split is not a shortcut: the browser rewrites contenteditable on every
/// keystroke, and letting Blazor's renderer diff against it corrupts content.</para>
/// <para><b>Uploads.</b> Wire <see cref="OnImageUpload"/> to store the file and return a
/// URL. With no handler the image is embedded as a data URI, which is fine for a
/// demo and a bad idea for real content.</para>
/// </remarks>
public partial class BzTextEditor : BzComponentBase, IAsyncDisposable
{
    [Inject] private IJSRuntime JS { get; set; } = default!;

    // ---------------- content ----------------

    /// <summary>The HTML content. Two-way bindable.</summary>
    [Parameter] public string? Value { get; set; }

    /// <summary>Raised as the user types.</summary>
    [Parameter] public EventCallback<string> ValueChanged { get; set; }

    /// <summary>Shown while the editor is empty.</summary>
    [Parameter] public string? Placeholder { get; set; }

    /// <summary>Label above the editor.</summary>
    [Parameter] public string? Label { get; set; }

    /// <summary>Marks the label with the required asterisk.</summary>
    [Parameter] public bool Required { get; set; }

    /// <summary>Help text under the editor.</summary>
    [Parameter] public string? Hint { get; set; }

    /// <summary>Validation message. Puts the editor in its error state.</summary>
    [Parameter] public string? Error { get; set; }

    // ---------------- behaviour ----------------

    /// <summary>Which toolbar tools to show. Combine <see cref="BzEditorTools"/> flags.</summary>
    [Parameter] public BzEditorTools Tools { get; set; } = BzEditorTools.Standard;

    /// <summary>Editing direction. <see cref="BzEditorDirection.Auto"/> follows the UI culture.</summary>
    [Parameter] public BzEditorDirection Direction { get; set; } = BzEditorDirection.Auto;

    /// <summary>Height of the editing area. Any CSS length.</summary>
    [Parameter] public string Height { get; set; } = "320px";

    /// <summary>Maximum height before the editor scrolls. Null lets it grow.</summary>
    [Parameter] public string? MaxHeight { get; set; }

    /// <summary>Disables editing but keeps the content visible.</summary>
    [Parameter] public bool ReadOnly { get; set; }

    /// <summary>Keeps the toolbar visible while the page scrolls.</summary>
    [Parameter] public bool StickyToolbar { get; set; } = true;

    /// <summary>Shows the word and character counter.</summary>
    [Parameter] public bool ShowStatusBar { get; set; } = true;

    /// <summary>Strips formatting from pasted content.</summary>
    [Parameter] public bool PlainTextPaste { get; set; }

    /// <summary>Soft character limit surfaced in the status bar. Null disables it.</summary>
    [Parameter] public int? MaxLength { get; set; }

    // ---------------- dropdown contents ----------------

    /// <summary>Fonts offered in the family dropdown. Sensible Persian + Latin defaults.</summary>
    [Parameter] public IReadOnlyList<BzEditorFont>? Fonts { get; set; }

    /// <summary>Sizes offered in the size dropdown, as CSS lengths.</summary>
    [Parameter]
    public IReadOnlyList<string> FontSizes { get; set; } =
        new[] { "12px", "14px", "16px", "18px", "20px", "24px", "30px", "36px", "48px" };

    /// <summary>Swatches offered by the colour pickers.</summary>
    [Parameter]
    public IReadOnlyList<string> Palette { get; set; } = new[]
    {
        "#000000", "#374151", "#6b7280", "#9ca3af", "#ffffff",
        "#ef4444", "#f97316", "#f59e0b", "#84cc16", "#22c55e",
        "#14b8a6", "#06b6d4", "#3b82f6", "#6366f1", "#8b5cf6",
        "#a855f7", "#ec4899", "#f43f5e", "#78350f", "#1e3a8a",
    };

    // ---------------- images ----------------

    /// <summary>
    /// Stores an uploaded image and returns its URL. Without a handler the image is
    /// inlined as a data URI.
    /// </summary>
    [Parameter] public Func<BzEditorUpload, Task<BzEditorUploadResult>>? OnImageUpload { get; set; }

    /// <summary>Largest image accepted, in megabytes.</summary>
    [Parameter] public int MaxImageSizeMB { get; set; } = 5;

    /// <summary>Accepted image types for the file picker.</summary>
    [Parameter] public string ImageAccept { get; set; } = "image/*";

    // ---------------- state ----------------

    private ElementReference _surface;
    private ElementReference _toolbar;
    private DotNetObjectReference<BzTextEditor>? _ref;
    private bool _ready;

    private BzEditorState _state = new();
    private BzEditorImageInfo _image = new();

    private string _html = "";
    private bool _source;
    private bool _fullscreen;
    private string? _uploadError;
    private bool _uploading;

    // open dropdown/dialog, null when none
    private string? _menu;

    // link dialog
    private bool _linkOpen;
    private string _linkUrl = "";
    private string _linkText = "";
    private bool _linkNewTab = true;

    // image dialog
    private bool _imageOpen;
    private string _imageUrl = "";
    private string _imageAlt = "";

    // table dialog
    private bool _tableOpen;
    private int _tableRows = 3;
    private int _tableCols = 3;
    private bool _tableHeader = true;

    private static bool IsFaCulture =>
        CultureInfo.CurrentUICulture.TwoLetterISOLanguageName is "fa" or "ar" or "he";

    private bool Rtl => Direction switch
    {
        BzEditorDirection.Rtl => true,
        BzEditorDirection.Ltr => false,
        _ => IsFaCulture,
    };

    private string DirAttr => Rtl ? "rtl" : "ltr";
    private bool Fa => IsFaCulture;

    private IReadOnlyList<BzEditorFont> FontList => Fonts ?? DefaultFonts;

    private static readonly BzEditorFont[] DefaultFonts =
    {
        new("پیش فرض", "inherit"),
        new("ایران سنس", "'IRANSans', system-ui, sans-serif"),
        new("وزیر", "'Vazirmatn', system-ui, sans-serif"),
        new("دانا", "'Dana', system-ui, sans-serif"),
        new("Arial", "Arial, Helvetica, sans-serif"),
        new("Georgia", "Georgia, serif"),
        new("Times", "'Times New Roman', Times, serif"),
        new("Courier", "'Courier New', Courier, monospace"),
    };

    private bool Has(BzEditorTools tool) => (Tools & tool) == tool;

    private bool HasAny(BzEditorTools group) => (Tools & group) != 0;

    protected override void OnParametersSet()
    {
        if (Value is not null && Value != _html && !_source)
        {
            _html = Value;
            if (_ready) _ = SetHtmlAsync(_html);
        }
    }

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (firstRender)
        {
            _ref = DotNetObjectReference.Create(this);
            var options = new { html = Value ?? "", plainPaste = PlainTextPaste };
            try
            {
                await JS.InvokeVoidAsync("bzTextEditor.init", Id, _surface, _ref, options);
                if (ReadOnly) await JS.InvokeVoidAsync("bzTextEditor.setReadOnly", Id, true);
                _ready = true;
            }
            catch (JSException)
            {
                // The Blazify bundle (or legacy bz-texteditor.js) is not on the page. Surface it rather than
                // silently rendering a dead box.
                _uploadError = Fa
                    ? "اسکریپت blazify.js بارگذاری نشده است."
                    : "blazify.js is not loaded.";
                StateHasChanged();
            }
            catch (InvalidOperationException)
            {
                // Prerendering: no JS yet. init runs again once interactive.
            }
        }
        else if (_reinitSurface && !_source)
        {
            // We just returned from source view. The old JS instance is bound to a
            // node Blazor threw away, so tear it down and bind a fresh one, seeded
            // with the html the user may have edited as text.
            _reinitSurface = false;
            try
            {
                await JS.InvokeVoidAsync("bzTextEditor.destroy", Id);
                _ref ??= DotNetObjectReference.Create(this);
                var options = new { html = _html ?? "", plainPaste = PlainTextPaste };
                await JS.InvokeVoidAsync("bzTextEditor.init", Id, _surface, _ref, options);
                if (ReadOnly) await JS.InvokeVoidAsync("bzTextEditor.setReadOnly", Id, true);
                _ready = true;
                if (ValueChanged.HasDelegate) await ValueChanged.InvokeAsync(_html ?? "");
            }
            catch (JSException) { }
            catch (InvalidOperationException) { }
        }
    }

    // ---------------- JS -> C# ----------------

    [JSInvokable]
    public async Task OnHtmlChangedAsync(string html)
    {
        _html = html;
        if (Value != html)
        {
            Value = html;
            if (ValueChanged.HasDelegate) await ValueChanged.InvokeAsync(html);
        }
    }

    [JSInvokable]
    public Task OnStateChangedAsync(string json)
    {
        try
        {
            var s = JsonSerializer.Deserialize<BzEditorState>(json, JsonOpts);
            if (s is not null) { _state = s; StateHasChanged(); }
        }
        catch (JsonException) { /* keep the last good state */ }
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task OnImageSelectedAsync(string json)
    {
        try
        {
            var i = JsonSerializer.Deserialize<BzEditorImageInfo>(json, JsonOpts);
            _image = i ?? new BzEditorImageInfo();
            StateHasChanged();
        }
        catch (JsonException) { }
        return Task.CompletedTask;
    }

    [JSInvokable]
    public Task OnFilesDroppedAsync()
    {
        // The browser hands dropped files to the drop target, not to InputFile, so
        // route the user to the same picker instead of a second upload path.
        _imageOpen = true;
        _menu = null;
        StateHasChanged();
        return Task.CompletedTask;
    }

    private static readonly JsonSerializerOptions JsonOpts = new()
    {
        PropertyNameCaseInsensitive = true,
    };

    // ---------------- C# -> JS ----------------

    private async Task ExecAsync(string command, string? value = null)
    {
        if (!_ready) return;
        await JS.InvokeVoidAsync("bzTextEditor.exec", Id, command, value);
    }

    private async Task SetHtmlAsync(string html)
    {
        if (!_ready) return;
        await JS.InvokeVoidAsync("bzTextEditor.setHtml", Id, html);
    }

    private async Task InsertAsync(string html)
    {
        if (!_ready) return;
        await JS.InvokeVoidAsync("bzTextEditor.insertHtml", Id, html);
    }

    private async Task SetDirectionAsync(string dir)
    {
        if (!_ready) return;
        await JS.InvokeVoidAsync("bzTextEditor.setDirection", Id, dir);
    }

    private async Task SetAlignAsync(string align)
    {
        if (!_ready) return;
        await JS.InvokeVoidAsync("bzTextEditor.setAlign", Id, align);
    }

    /// <summary>Replaces the content programmatically.</summary>
    public async Task SetContentAsync(string html)
    {
        _html = html;
        Value = html;
        await SetHtmlAsync(html);
        if (ValueChanged.HasDelegate) await ValueChanged.InvokeAsync(html);
    }

    /// <summary>Reads the current HTML straight from the DOM.</summary>
    public async Task<string> GetContentAsync()
        => _ready ? await JS.InvokeAsync<string>("bzTextEditor.getHtml", Id) : _html;

    /// <summary>Puts the caret back in the editor.</summary>
    public async Task FocusAsync()
    {
        if (_ready) await JS.InvokeVoidAsync("bzTextEditor.focus", Id);
    }

    // ---------------- toolbar actions ----------------

    private void ToggleMenu(string name) => _menu = _menu == name ? null : name;

    private async Task ApplyBlock(string tag)
    {
        _menu = null;
        await ExecAsync("formatBlock", $"<{tag}>");
    }

    private async Task ApplyFont(string stack)
    {
        _menu = null;
        // fontName does not take a stack, so wrap the selection ourselves.
        await ExecAsync("fontName", stack);
    }

    private async Task ApplyFontSize(string size)
    {
        _menu = null;
        await ExecAsync("bzFontSize", size);
    }

    private async Task ApplyLineHeight(string lh)
    {
        _menu = null;
        await ExecAsync("bzLineHeight", lh);
    }

    private async Task ApplyColor(string color)
    {
        _menu = null;
        await ExecAsync("foreColor", color);
    }

    private async Task ApplyHighlight(string color)
    {
        _menu = null;
        await ExecAsync("hiliteColor", color);
    }

    private async Task ClearFormatting()
    {
        await ExecAsync("removeFormat");
        await ExecAsync("formatBlock", "<p>");
    }

    private async Task InsertHr() => await InsertAsync("<hr />");

    private async Task InsertQuote() => await ExecAsync("formatBlock", "<blockquote>");

    private async Task InsertCode() => await ExecAsync("formatBlock", "<pre>");

    private bool _reinitSurface;

    private async Task ToggleSource()
    {
        if (!_source)
        {
            // going TO source view: pull the latest html out of the live editor
            _html = await GetContentAsync();
            Value = _html;
            _source = true;
        }
        else
        {
            // coming BACK: Blazor re-creates the contenteditable div, so the JS
            // instance is now bound to a dead node. Flag a re-init for the next
            // render and let OnAfterRenderAsync rebuild it with the current html.
            _source = false;
            _reinitSurface = true;
        }
        StateHasChanged();
    }

    private async Task OnSourceInput(ChangeEventArgs e)
    {
        _html = e.Value?.ToString() ?? "";
        Value = _html;
        if (ValueChanged.HasDelegate) await ValueChanged.InvokeAsync(_html);
    }

    private void ToggleFullscreen() => _fullscreen = !_fullscreen;

    private async Task PrintAsync()
    {
        if (_ready) await JS.InvokeVoidAsync("bzTextEditor.print", Id);
    }

    // ---------------- link ----------------

    private async Task OpenLink()
    {
        _menu = null;
        if (_ready) await JS.InvokeVoidAsync("bzTextEditor.saveSelection", Id);
        _linkUrl = "";
        _linkText = "";
        _linkOpen = true;
    }

    private async Task ConfirmLink()
    {
        _linkOpen = false;
        if (string.IsNullOrWhiteSpace(_linkUrl)) return;

        var url = Escape(_linkUrl.Trim());
        var target = _linkNewTab ? " target=\"_blank\" rel=\"noopener noreferrer\"" : "";
        var text = string.IsNullOrWhiteSpace(_linkText) ? url : Escape(_linkText);

        await InsertAsync($"<a href=\"{url}\"{target}>{text}</a>");
    }

    private async Task RemoveLink() => await ExecAsync("unlink");

    // ---------------- image ----------------

    private async Task OpenImage()
    {
        _menu = null;
        if (_ready) await JS.InvokeVoidAsync("bzTextEditor.saveSelection", Id);
        _imageUrl = "";
        _imageAlt = "";
        _uploadError = null;
        _imageOpen = true;
    }

    private async Task ConfirmImageUrl()
    {
        _imageOpen = false;
        if (string.IsNullOrWhiteSpace(_imageUrl)) return;
        await InsertImage(_imageUrl.Trim(), _imageAlt);
    }

    private async Task OnImagePicked(InputFileChangeEventArgs e)
    {
        var file = e.File;
        _uploadError = null;

        var max = (long)MaxImageSizeMB * 1024 * 1024;
        if (file.Size > max)
        {
            _uploadError = Fa
                ? $"حجم فایل بیشتر از {MaxImageSizeMB} مگابایت است."
                : $"The file is larger than {MaxImageSizeMB} MB.";
            return;
        }

        _uploading = true;
        StateHasChanged();

        try
        {
            string? url;
            if (OnImageUpload is not null)
            {
                var result = await OnImageUpload(new BzEditorUpload { File = file });
                if (result.Url is null)
                {
                    _uploadError = result.Error ?? (Fa ? "بارگذاری ناموفق بود." : "Upload failed.");
                    return;
                }
                url = result.Url;
            }
            else
            {
                // No handler: inline it. Fine for demos, not for real content —
                // data URIs bloat the HTML and are never cached.
                await using var stream = file.OpenReadStream(max);
                using var ms = new MemoryStream();
                await stream.CopyToAsync(ms);
                url = $"data:{file.ContentType};base64,{Convert.ToBase64String(ms.ToArray())}";
            }

            _imageOpen = false;
            await InsertImage(url, Path.GetFileNameWithoutExtension(file.Name));
        }
        catch (Exception ex)
        {
            _uploadError = ex.Message;
        }
        finally
        {
            _uploading = false;
            StateHasChanged();
        }
    }

    private async Task InsertImage(string url, string? alt)
    {
        var safeUrl = Escape(url);
        var safeAlt = Escape(alt ?? "");
        await InsertAsync($"<img src=\"{safeUrl}\" alt=\"{safeAlt}\" class=\"bz-te-img\" />");
    }

    private async Task ApplyImageProps()
    {
        if (!_ready) return;
        var json = JsonSerializer.Serialize(new
        {
            alt = _image.Alt ?? "",
            width = _image.Width ?? "",
            align = _image.Align ?? "none",
            caption = _image.Caption ?? "",
        });
        await JS.InvokeVoidAsync("bzTextEditor.applyImage", Id, json);
    }

    private async Task SetImageWidth(string width)
    {
        _image.Width = width;
        await ApplyImageProps();
    }

    private async Task SetImageAlign(string align)
    {
        _image.Align = align;
        await ApplyImageProps();
    }

    private async Task RemoveImage()
    {
        if (_ready) await JS.InvokeVoidAsync("bzTextEditor.removeImage", Id);
    }

    // ---------------- table ----------------

    private async Task OpenTable()
    {
        _menu = null;
        if (_ready) await JS.InvokeVoidAsync("bzTextEditor.saveSelection", Id);
        _tableOpen = true;
    }

    private async Task ConfirmTable()
    {
        _tableOpen = false;
        var rows = Math.Clamp(_tableRows, 1, 30);
        var cols = Math.Clamp(_tableCols, 1, 12);

        var sb = new System.Text.StringBuilder();
        sb.Append("<table class=\"bz-te-table\"><tbody>");

        if (_tableHeader)
        {
            sb.Append("<tr>");
            for (var c = 0; c < cols; c++) sb.Append("<th><br /></th>");
            sb.Append("</tr>");
            rows--;
        }
        for (var r = 0; r < rows; r++)
        {
            sb.Append("<tr>");
            for (var c = 0; c < cols; c++) sb.Append("<td><br /></td>");
            sb.Append("</tr>");
        }
        sb.Append("</tbody></table><p><br /></p>");

        await InsertAsync(sb.ToString());
    }

    // ---------------- misc ----------------

    /// <summary>Minimal HTML attribute escaping for values we build ourselves.</summary>
    private static string Escape(string s) => s
        .Replace("&", "&amp;")
        .Replace("\"", "&quot;")
        .Replace("<", "&lt;")
        .Replace(">", "&gt;");

    private bool OverLimit => MaxLength.HasValue && _state.Chars > MaxLength.Value;

    public async ValueTask DisposeAsync()
    {
        try
        {
            if (_ready) await JS.InvokeVoidAsync("bzTextEditor.destroy", Id);
        }
        catch (JSDisconnectedException) { /* circuit already gone */ }
        catch (JSException) { }

        _ref?.Dispose();
        base.Dispose();
        GC.SuppressFinalize(this);
    }
}
