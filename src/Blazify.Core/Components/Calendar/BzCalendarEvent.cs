using BzCore.Enums;

namespace BzCore.Components.Calendar;

public sealed class BzCalendarEvent
{
    /// <summary>Event day</summary>
    public DateTime Date { get; set; }
    /// <summary>Event title</summary>
    public string Title { get; set; } = "";
    /// <summary>Event color on calendar</summary>
    public BzColor Color { get; set; } = BzColor.Primary;

    public BzCalendarEvent() { }

    public BzCalendarEvent(DateTime date, string title, BzColor color = BzColor.Primary)
    {
        Date = date; Title = title; Color = color;
    }
}
