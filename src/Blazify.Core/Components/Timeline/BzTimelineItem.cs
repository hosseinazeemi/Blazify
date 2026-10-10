using BzCore.Enums;

namespace BzCore.Components.Timeline;

/// <summary>One entry in a <see cref="BzTimeline"/>.</summary>
public sealed class BzTimelineItem
{
    /// <summary>Entry title.</summary>
    public string Title { get; set; } = "";
    /// <summary>Optional description.</summary>
    public string? Text { get; set; }
    /// <summary>Optional time/date label.</summary>
    public string? Time { get; set; }
    /// <summary>Optional marker icon (font-awesome class).</summary>
    public string? Icon { get; set; }
    /// <summary>Marker colour.</summary>
    public BzColor Color { get; set; } = BzColor.Primary;

    public BzTimelineItem() { }

    public BzTimelineItem(string title, string? text = null, string? time = null,
                          string? icon = null, BzColor color = BzColor.Primary)
    {
        Title = title; Text = text; Time = time; Icon = icon; Color = color;
    }
}
