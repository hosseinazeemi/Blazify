using System.Globalization;
using BzCore.Enums;

namespace BzCore.Helpers;

public static class CalendarHelper
{
    private static readonly PersianCalendar Pc = new();

    public static readonly string[] WeekdayHeaders = { "ش", "ی", "د", "س", "چ", "پ", "ج" };

    private static readonly string[] JalaliMonths =
    {
        "فروردین", "اردیبهشت", "خرداد", "تیر", "مرداد", "شهریور",
        "مهر", "آبان", "آذر", "دی", "بهمن", "اسفند"
    };

    private static readonly string[] GregorianMonths =
    {
        "ژانویه", "فوریه", "مارس", "آوریل", "می", "ژوئن",
        "ژوئیه", "اوت", "سپتامبر", "اکتبر", "نوامبر", "دسامبر"
    };

    public static (int year, int month, int day) GetParts(DateTime dt, BzCalendarType cal)
        => cal == BzCalendarType.Jalali
            ? (Pc.GetYear(dt), Pc.GetMonth(dt), Pc.GetDayOfMonth(dt))
            : (dt.Year, dt.Month, dt.Day);

    public static DateTime ToDateTime(int year, int month, int day, BzCalendarType cal)
    {
        try
        {
            return cal == BzCalendarType.Jalali
                ? Pc.ToDateTime(year, month, day, 0, 0, 0, 0)
                : new DateTime(year, month, day);
        }
        catch
        {
            return DateTime.Today;
        }
    }

    public static int DaysInMonth(int year, int month, BzCalendarType cal)
        => cal == BzCalendarType.Jalali ? Pc.GetDaysInMonth(year, month) : DateTime.DaysInMonth(year, month);

    public static string MonthName(int month, BzCalendarType cal)
        => cal == BzCalendarType.Jalali ? JalaliMonths[month - 1] : GregorianMonths[month - 1];

    public static int WeekColumn(DateTime dt) => ((int)dt.DayOfWeek + 1) % 7;

    public static List<DateTime> BuildMonthGrid(int year, int month, BzCalendarType cal)
    {
        var first = ToDateTime(year, month, 1, cal);
        var leading = WeekColumn(first);
        var start = first.AddDays(-leading);

        var grid = new List<DateTime>(42);
        for (var i = 0; i < 42; i++)
            grid.Add(start.AddDays(i));
        return grid;
    }

    public static (int year, int month) AddMonths(int year, int month, int delta, BzCalendarType cal)
    {
        var total = (year * 12 + (month - 1)) + delta;
        var ny = total / 12;
        var nm = total % 12 + 1;
        return (ny, nm);
    }

    public static string FormatDate(DateTime dt, BzCalendarType cal, bool persianDigits = false)
    {
        var (y, m, d) = GetParts(dt, cal);
        var s = $"{y:0000}/{m:00}/{d:00}";
        return persianDigits ? PersianHelper.ToPersianDigits(s) : s;
    }

    public static bool SameDay(DateTime a, DateTime b)
        => a.Year == b.Year && a.Month == b.Month && a.Day == b.Day;

    public static bool InMonth(DateTime dt, int year, int month, BzCalendarType cal)
    {
        var (y, m, _) = GetParts(dt, cal);
        return y == year && m == month;
    }
}
