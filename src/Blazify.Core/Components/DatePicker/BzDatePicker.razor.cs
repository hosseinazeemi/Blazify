using Microsoft.AspNetCore.Components;
using Blazify.Core.Base;
using BzCore.Enums;
using BzCore.Helpers;
using System;
using System.Linq;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.JSInterop;

namespace BzCore.Components.DatePicker;
public partial class BzDatePicker : BzFieldBase<DateTime?>
{
    private bool _open;
    private bool _viewInitialised;
    private int _viewYear;
    private int _viewMonth;
    private bool _selectingEnd;
    private bool _dropUp;
    [Inject] private IJSRuntime JS { get; set; } = default!;

    [Parameter] public BzCalendarType Calendar { get; set; } = BzCalendarType.Jalali;
    /// <summary>
    /// Range DatePicker
    /// </summary>
    [Parameter] public bool Range { get; set; }
    /// <summary>In range mode : until further notice | ta etelaa sanavi</summary>
    [Parameter] public bool AllowUntilFurtherNotice { get; set; }
    [Parameter] public bool PersianDigits { get; set; }

    /// <summary>min selectable date</summary>
    [Parameter] public DateTime? Min { get; set; }
    /// <summary>max selectable date</summary>
    [Parameter] public DateTime? Max { get; set; }

    /// <summary>Dates after today are disabled by default.</summary>
    [Parameter] public bool DisableFutureDates { get; set; } = true;
    /// <summary>Disable every date strictly before this boundary.</summary>
    [Parameter] public DateTime? DisableBefore { get; set; }
    /// <summary>Disable every date strictly after this boundary.</summary>
    [Parameter] public DateTime? DisableAfter { get; set; }

    [Parameter] public DateTime? Today { get; set; }

    private DateTime CurrentDate => (Today ?? DateTime.Today).Date;

    /// <summary>
    /// tWo way binding
    /// </summary>
    [Parameter] public DateTime? EndValue { get; set; }
    [Parameter] public EventCallback<DateTime?> EndValueChanged { get; set; }

    [Parameter] public bool IsOpenEnded { get; set; }
    [Parameter] public EventCallback<bool> IsOpenEndedChanged { get; set; }

    [Parameter] public EventCallback OnClosed { get; set; }

    private string WrapperClass => RootCss("bz-dp")
        .Add($"bz-dp--{Size.ToString().ToLowerInvariant()}")
        .AddIf(IsDisabled, "is-disabled")
        .AddIf(IsReadOnly, "is-readonly")
        .AddIf(_open, "is-open")
        .AddIf(_dropUp, "is-drop-up")
        .AddIf(HasError, "is-invalid")
        .Build();

    private string DisplayText
    {
        get
        {
            if (Value is null) return string.Empty;
            var left = CalendarHelper.FormatDate(Value.Value, Calendar, PersianDigits);
            if (!Range) return left;

            string right = IsOpenEnded
                ? "تا اطلاع ثانوی"
                : EndValue is not null ? CalendarHelper.FormatDate(EndValue.Value, Calendar, PersianDigits) : "…";
            return $"{left}  —  {right}";
        }
    }

    private string HeaderTitle
    {
        get
        {
            if (_pick == PickMode.Months)
                return PersianDigits ? PersianHelper.ToPersianDigits(_viewYear.ToString()) : _viewYear.ToString();
            if (_pick == PickMode.Years)
            {
                var a = _yearPageStart; var b = _yearPageStart + 11;
                return PersianDigits ? $"{PersianHelper.ToPersianDigits(a.ToString())} - {PersianHelper.ToPersianDigits(b.ToString())}"
                                     : $"{a} - {b}";
            }
            var name = CalendarHelper.MonthName(_viewMonth, Calendar);
            var year = PersianDigits ? PersianHelper.ToPersianDigits(_viewYear.ToString()) : _viewYear.ToString();
            return $"{name} {year}";
        }
    }

    private enum PickMode 
    { 
        Days,
        Months, 
        Years 
    }
    private PickMode _pick = PickMode.Days;
    private int _yearPageStart;

    private string MonthLabel(int m) => CalendarHelper.MonthName(m, Calendar);
    private string YearLabel(int y) => PersianDigits ? PersianHelper.ToPersianDigits(y.ToString()) : y.ToString();
    private IEnumerable<int> YearGrid => Enumerable.Range(_yearPageStart, 12);

    private void HeaderClick()
    {
        if (_pick == PickMode.Days) _pick = PickMode.Months;
        else if (_pick == PickMode.Months) { _yearPageStart = _viewYear - 6; _pick = PickMode.Years; }
        else _pick = PickMode.Months;
        StateHasChanged();
    }

    private void HeaderNav(int dir)
    {
        if (_pick == PickMode.Days) MoveYear(dir);
        else if (_pick == PickMode.Months) { _viewYear += dir; StateHasChanged(); }
        else { _yearPageStart += dir * 12; StateHasChanged(); }
    }

    private void PickMonth(int m) 
    { 
        _viewMonth = m;
        _pick = PickMode.Days; 
        StateHasChanged();
    }
    private void PickYear(int y) 
    { 
        _viewYear = y;
        _pick = PickMode.Months;
        StateHasChanged();
    }

    protected override void OnFieldParametersSet()
    {
        if (!_viewInitialised)
        {
            var basis = Value ?? CurrentDate;
            var (y, m, _) = CalendarHelper.GetParts(basis, Calendar);
            _viewYear = y; _viewMonth = m;
            _viewInitialised = true;
        }
    }

    private void Toggle()
    {
        if (IsDisabled || IsReadOnly) return;
        _open = !_open;
        if (_open) 
        { 
            _pick = PickMode.Days;
            SyncViewToValue(); 
        }
        StateHasChanged();
    }

    private void Close()
    {
        if (!_open) return;
        _open = false;
        StateHasChanged();
        _ = OnClosed.InvokeAsync();
    }

    private void SyncViewToValue()
    {
        var basis = Value ?? CurrentDate;
        var (y, m, _) = CalendarHelper.GetParts(basis, Calendar);
        _viewYear = y; 
        _viewMonth = m;
    }

    private void Move(int months)
    {
        (_viewYear, _viewMonth) = CalendarHelper.AddMonths(_viewYear, _viewMonth, months, Calendar);
        StateHasChanged();
    }

    private void MoveYear(int years) => Move(years * 12);

    private bool IsDisabledDate(DateTime dt)
    {
        var date = dt.Date;
        var minimum = DisableBefore?.Date ?? Min?.Date;
        var maximum = DisableAfter?.Date ?? Max?.Date;
        if (DisableFutureDates)
            maximum = maximum is null || CurrentDate < maximum ? CurrentDate : maximum;

        return (minimum is not null && date < minimum) ||
               (maximum is not null && date > maximum);
    }

    private async Task SelectAsync(DateTime dt)
    {
        if (IsDisabledDate(dt)) return;

        if (!Range)
        {
            await SetValueAsync(dt.Date);
            Close();
            return;
        }

        if (Value is null || !_selectingEnd)
        {
            await SetValueAsync(dt.Date);
            await SetEndAsync(null);
            await SetOpenEndedAsync(false);
            _selectingEnd = true;
        }
        else
        {
            if (dt.Date < Value.Value.Date)
            {
                await SetValueAsync(dt.Date);
            }
            else
            {
                await SetEndAsync(dt.Date);
                _selectingEnd = false;
                Close();
            }
        }
        StateHasChanged();
    }

    private async Task SelectTodayAsync() => await SelectAsync(CurrentDate);

    private async Task ClearAsync()
    {
        await SetValueAsync(null);
        await SetEndAsync(null);
        await SetOpenEndedAsync(false);
        _selectingEnd = false;
        StateHasChanged();
    }

    private async Task ToggleUntilFurtherNoticeAsync()
    {
        var next = !IsOpenEnded;
        await SetOpenEndedAsync(next);
        if (next) { await SetEndAsync(null); _selectingEnd = false; Close(); }
    }

    private async Task SetEndAsync(DateTime? value)
    {
        EndValue = value;
        await EndValueChanged.InvokeAsync(value);
    }

    private async Task SetOpenEndedAsync(bool value)
    {
        IsOpenEnded = value;
        await IsOpenEndedChanged.InvokeAsync(value);
    }

    private string CellClass(DateTime dt)
    {
        var css = CssBuilder.Default("bz-dp__cell");

        if (!CalendarHelper.InMonth(dt, _viewYear, _viewMonth, Calendar))
            css = css.Add("is-outside");

        if (IsDisabledDate(dt))
            css = css.Add("is-disabled");

        if (CalendarHelper.SameDay(dt, CurrentDate))
            css = css.Add("is-today");

        var isStart = Value is not null && CalendarHelper.SameDay(dt, Value.Value);
        var isEnd = EndValue is not null && CalendarHelper.SameDay(dt, EndValue.Value);

        if (isStart) css = css.Add(Range ? "is-range-start is-selected" : "is-selected");
        if (isEnd) css = css.Add("is-range-end is-selected");

        if (Range && Value is not null && EndValue is not null &&
            dt.Date > Value.Value.Date && dt.Date < EndValue.Value.Date)
            css = css.Add("in-range");

        return css.Build();
    }

    private string DayLabel(DateTime dt)
    {
        var (_, _, d) = CalendarHelper.GetParts(dt, Calendar);
        return PersianDigits ? PersianHelper.ToPersianDigits(d.ToString()) : d.ToString();
    }

    private List<DateTime> Grid => CalendarHelper.BuildMonthGrid(_viewYear, _viewMonth, Calendar);

    public void Open() 
    { 
        if (!IsDisabled && !IsReadOnly) 
        {
            _open = true;
            SyncViewToValue();
            StateHasChanged();
        }
    }
    public void CloseCalendar() => Close();
    public Task ClearAsyncPublic() => ClearAsync();

    protected override async Task OnAfterRenderAsync(bool firstRender)
    {
        if (_open)
        {
            try
            {
                var next = await JS.InvokeAsync<bool>("bzOverlay.shouldOpenUp", Id, 390);
                if (next != _dropUp) { _dropUp = next; StateHasChanged(); }
            }
            catch (JSException) { }
        }
    }
}
