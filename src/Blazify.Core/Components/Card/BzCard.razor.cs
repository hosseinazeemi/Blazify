using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Web;
using Blazify.Core.Base;
using BzCore.Enums;
using BzCore.Helpers;

namespace BzCore.Components.Card;
public partial class BzCard : BzComponentBase
{
    [Parameter] public BzCardVariant Variant { get; set; } = BzCardVariant.Elevated;
    [Parameter] public BzColor Color { get; set; } = BzColor.Primary;

    [Parameter] public string? Title { get; set; }
    [Parameter] public string? Subtitle { get; set; }
    /// <summary>Font Awesome Icons</summary>
    [Parameter] public string? Icon { get; set; }

    [Parameter] public string? ImageUrl { get; set; }
    [Parameter] public RenderFragment? Media { get; set; }
    [Parameter] public RenderFragment? Header { get; set; }
    [Parameter] public RenderFragment? ChildContent { get; set; }
    [Parameter] public RenderFragment? Footer { get; set; }
    [Parameter] public RenderFragment? Actions { get; set; }

    [Parameter] public string? Width { get; set; }
    [Parameter] public bool Hoverable { get; set; }
    [Parameter] public bool Clickable { get; set; }
    [Parameter] public EventCallback<MouseEventArgs> OnClick { get; set; }

    private string CardClass => RootCss("bz-card")
        .Add($"bz-card--{Variant.ToString().ToLowerInvariant()}")
        .Add($"bz-card--c-{Color.ToString().ToLowerInvariant()}")
        .AddIf(Hoverable || Clickable, "is-hoverable")
        .AddIf(Clickable, "is-clickable")
        .AddIf(IsDense, "is-dense")
        .Build();

    private string CardStyle =>
        $"--bz-card-c:{ColorHelper.Var(Color)};--bz-card-on:{ColorHelper.OnVar(Color)};" +
        (Width is not null ? $"width:{Width};" : "") + (Style ?? "");

    private bool HasHeader => Header is not null || !string.IsNullOrEmpty(Title) || !string.IsNullOrEmpty(Icon);
    private bool HasMedia  => Media is not null || !string.IsNullOrEmpty(ImageUrl);

    private async Task HandleClick(MouseEventArgs e)
    {
        if (Clickable && OnClick.HasDelegate) await OnClick.InvokeAsync(e);
    }
}
