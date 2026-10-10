using Microsoft.AspNetCore.Components;
using BzCore.Enums;

namespace BzCore.Notifications;

public sealed class BzNotification
{
    public Guid Id { get; } = Guid.NewGuid();
    public BzSeverity Severity { get; set; } = BzSeverity.Info;
    public string? Title { get; set; }
    public string? Message { get; set; }

    public string? Icon { get; set; }

    public int Duration { get; set; } = 4000;

    public bool Closable { get; set; } = true;
    public bool ShowProgress { get; set; } = true;

    public string? Group { get; set; }

    public RenderFragment? Content { get; set; }

    public string ResolvedIcon => Icon ?? SeverityHelper.Icon(Severity);
}

public static class SeverityHelper
{
    public static BzColor Color(BzSeverity s) => s switch
    {
        BzSeverity.Success => BzColor.Success,
        BzSeverity.Warning => BzColor.Warning,
        BzSeverity.Error => BzColor.Danger,
        _ => BzColor.Info
    };

    public static string Icon(BzSeverity s) => s switch
    {
        BzSeverity.Success => "fa-solid fa-circle-check",
        BzSeverity.Warning => "fa-solid fa-triangle-exclamation",
        BzSeverity.Error => "fa-solid fa-circle-xmark",
        _ => "fa-solid fa-circle-info"
    };

    public static string Cls(BzSeverity s) => s.ToString().ToLowerInvariant();
}

public sealed class BzRunOptions
{
    public string? Pending { get; set; }

    public string Success { get; set; } = "عملیات با موفقیت انجام شد";
    public BzSeverity SuccessSeverity { get; set; } = BzSeverity.Success;

    public string? Error { get; set; }
    public BzSeverity ErrorSeverity { get; set; } = BzSeverity.Error;

    public bool ShowSuccess { get; set; } = true;
    public bool ShowError { get; set; } = true;
    public int Duration { get; set; } = 4000;
}
