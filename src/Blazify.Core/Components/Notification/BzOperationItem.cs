using BzCore.Enums;

namespace BzCore.Components.Notification;

public sealed class BzOperationItem
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");
    /// <summary>Status, sets the color, icon and group</summary>
    public BzOpStatus Status { get; set; } = BzOpStatus.Info;
    /// <summary>Message text</summary>
    public string Text { get; set; } = string.Empty;
    /// <summary>Optional bold title</summary>
    public string? Title { get; set; }
    /// <summary>When the message was created (HH:mm)</summary>
    public DateTime Time { get; set; } = DateTime.Now;

    public BzOperationItem() { }

    public BzOperationItem(BzOpStatus status, string text, string? title = null)
    {
        Status = status;
        Text = text;
        Title = title;
    }
}
