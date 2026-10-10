using System.Globalization;
using Microsoft.AspNetCore.Components;
using Blazify.Core.Base;
using BzCore.Enums;
using System.Collections.Generic;
using System.Linq;
using System;

namespace BzCore.Components.Chart;

public partial class BzChart : BzComponentBase
{
    private const double W = 640;   // viewBox width  (drawing units, not pixels)
    private const double H = 360;   // viewBox height
    private static readonly CultureInfo Inv = CultureInfo.InvariantCulture;

    /// <summary>Chart kind (Bar, Line, Area, Sparkline, Pie, Donut).</summary>
    [Parameter] public BzChartType Type { get; set; } = BzChartType.Bar;
    /// <summary>The data series to plot.</summary>
    [Parameter] public List<BzChartSeries> Series { get; set; } = new();
    /// <summary>Category labels for the X axis / pie slices.</summary>
    [Parameter] public List<string>? Labels { get; set; }

    /// <summary>Rendered height (the SVG scales to fit).</summary>
    [Parameter] public string Height { get; set; } = "320px";

    /// <summary>Show the series/category legend.</summary>
    [Parameter] public bool ShowLegend { get; set; } = true;
    /// <summary>Show horizontal grid lines (cartesian charts).</summary>
    [Parameter] public bool ShowGrid { get; set; } = true;
    /// <summary>Print each value on the chart.</summary>
    [Parameter] public bool ShowValues { get; set; }
    /// <summary>Animate on render.</summary>
    [Parameter] public bool Animate { get; set; } = true;
    /// <summary>Optional chart title.</summary>
    [Parameter] public string? Title { get; set; }

    private static readonly string[] Palette =
    {
        "var(--bz-primary)", "var(--bz-success)", "var(--bz-warning)",
        "var(--bz-info)", "var(--bz-danger)", "var(--bz-secondary)"
    };

    private string Color(int i, string? overrideColor) => overrideColor ?? Palette[i % Palette.Length];
    private static string F(double v) => v.ToString("0.##", Inv);

    private bool IsCartesian => Type is BzChartType.Bar or BzChartType.Line or BzChartType.Area or BzChartType.Sparkline;
    private bool IsRadial => Type is BzChartType.Pie or BzChartType.Donut;

    private double PadLeft => Type == BzChartType.Sparkline ? 0 : 44;
    private double PadBottom => Type == BzChartType.Sparkline ? 0 : 28;
    private const double PadTop = 12;
    private const double PadRight = 12;
    private double PlotX => PadLeft;
    private double PlotY => PadTop;
    private double PlotW => W - PadLeft - PadRight;
    private double PlotH => H - PadTop - PadBottom;

    private int Count => Labels?.Count ?? (Series.FirstOrDefault()?.Data.Count ?? 0);

    private double MaxValue
    {
        get
        {
            var max = Series.SelectMany(s => s.Data).DefaultIfEmpty(0).Max();
            return max <= 0 ? 1 : max;
        }
    }

    // Round the axis maximum up to a "nice" number (1/2/5 x 10^n) so gridline labels
    // read cleanly instead of e.g. 37.3, 74.6, ...
    private double NiceMax(int ticks = 4)
    {
        var max = MaxValue;
        var raw = max / ticks;
        var mag = Math.Pow(10, Math.Floor(Math.Log10(raw <= 0 ? 1 : raw)));
        var norm = raw / mag;
        double nice = norm <= 1 ? 1 : norm <= 2 ? 2 : norm <= 5 ? 5 : 10;
        return nice * mag * ticks;
    }

    // Map a data value to a Y pixel inside the plot area (0 at the bottom, max at top).
    private double Y(double value) => PlotY + PlotH - (value / NiceMax()) * PlotH;
    private double BandWidth => Count == 0 ? PlotW : PlotW / Count;
    private double BandCenter(int i) => PlotX + BandWidth * i + BandWidth / 2;

    private record GridLine(double Yc, string Label);
    private IEnumerable<GridLine> GridLines(int ticks = 4)
    {
        var niceMax = NiceMax(ticks);
        for (var t = 0; t <= ticks; t++)
        {
            var val = niceMax / ticks * t;
            yield return new GridLine(Y(val), Trim(val));
        }
    }
    private static string Trim(double v) => v >= 1000 ? (v / 1000).ToString("0.#", Inv) + "k" : v.ToString("0.#", Inv);

    private IEnumerable<(double X, string Text)> XLabels()
    {
        if (Labels is null) yield break;
        for (var i = 0; i < Labels.Count; i++)
            yield return (BandCenter(i), Labels[i]);
    }

    private record Bar(double X, double Yc, double Width, double Height, string Color, double Value, double Delay);
    private IEnumerable<Bar> Bars()
    {
        var s = Series.Count;
        if (s == 0) yield break;
        var groupW = BandWidth * 0.7;
        var barW = groupW / s;
        var startOffset = (BandWidth - groupW) / 2;

        for (var si = 0; si < s; si++)
        {
            var color = Color(si, Series[si].Color);
            for (var i = 0; i < Series[si].Data.Count; i++)
            {
                var v = Series[si].Data[i];
                var x = PlotX + BandWidth * i + startOffset + barW * si;
                var yTop = Y(v);
                var h = PlotY + PlotH - yTop;
                yield return new Bar(x, yTop, barW, h, color, v, i * 0.05);
            }
        }
    }

    private record SeriesPath(string Points, string AreaPath, string Color, List<(double X, double Y)> Dots);
    private IEnumerable<SeriesPath> Lines()
    {
        for (var si = 0; si < Series.Count; si++)
        {
            var data = Series[si].Data;
            var pts = new List<(double X, double Y)>();
            for (var i = 0; i < data.Count; i++)
            {
                var x = Type == BzChartType.Sparkline
                    ? PlotX + (data.Count <= 1 ? 0 : PlotW * i / (data.Count - 1))
                    : BandCenter(i);
                pts.Add((x, Y(data[i])));
            }

            var polyline = string.Join(" ", pts.Select(p => $"{F(p.X)},{F(p.Y)}"));
            string area = "";
            if (pts.Count > 0)
            {
                var baseY = PlotY + PlotH;
                area = $"M {F(pts[0].X)},{F(baseY)} " +
                       string.Join(" ", pts.Select(p => $"L {F(p.X)},{F(p.Y)}")) +
                       $" L {F(pts[^1].X)},{F(baseY)} Z";
            }
            yield return new SeriesPath(polyline, area, Color(si, Series[si].Color), pts);
        }
    }

    private double Radius => Math.Min(W / 2, H / 2) - 30;
    private record Slice(string Path, string Color, string Label, double Percent, double LabelX, double LabelY);
    private IEnumerable<Slice> Slices()
    {
        var data = Series.FirstOrDefault()?.Data ?? new List<double>();
        var total = data.Sum();
        if (total <= 0) yield break;

        double cx = W / 2, cy = H / 2;
        var r = Radius;

        double angle = -90; // start at top
        for (var i = 0; i < data.Count; i++)
        {
            var frac = data[i] / total;
            var sweep = frac * 360;
            var end = angle + sweep;

            string path;
            if (sweep >= 359.999)
            {
                path = $"M {F(cx)},{F(cy - r)} A {F(r)},{F(r)} 0 1 1 {F(cx - 0.01)},{F(cy - r)} Z";
            }
            else
            {
                var (x1, y1) = Polar(cx, cy, r, angle);
                var (x2, y2) = Polar(cx, cy, r, end);
                var large = sweep > 180 ? 1 : 0;
                path = $"M {F(cx)},{F(cy)} L {F(x1)},{F(y1)} A {F(r)},{F(r)} 0 {large} 1 {F(x2)},{F(y2)} Z";
            }

            var mid = angle + sweep / 2;
            var (lx, ly) = Polar(cx, cy, r * 0.62, mid);
            var label = Labels is not null && i < Labels.Count ? Labels[i] : "";
            yield return new Slice(path, Color(i, null), label, frac * 100, lx, ly);
            angle = end;
        }
    }

    private static (double X, double Y) Polar(double cx, double cy, double r, double angleDeg)
    {
        var a = angleDeg * Math.PI / 180.0;
        return (cx + r * Math.Cos(a), cy + r * Math.Sin(a));
    }

    private double DonutHoleRadius => Radius * 0.55;

    private string RootClass => RootCss("bz-chart").Build();
    private string ViewBox => $"0 0 {F(W)} {F(H)}";

    private IEnumerable<(string Color, string Label)> Legend()
    {
        if (IsRadial)
        {
            var data = Series.FirstOrDefault()?.Data ?? new();
            for (var i = 0; i < data.Count; i++)
                yield return (Color(i, null), Labels is not null && i < Labels.Count ? Labels[i] : $"#{i + 1}");
        }
        else
        {
            for (var i = 0; i < Series.Count; i++)
                yield return (Color(i, Series[i].Color), Series[i].Label ?? $"سری {i + 1}");
        }
    }
}
