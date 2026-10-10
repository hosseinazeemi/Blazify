namespace BzCore.Components.Chart;

public sealed class BzChartSeries
{
    /// <summary>Series name (shown in the legend).</summary>
    public string? Label { get; set; }
    public string? Color { get; set; }
    public List<double> Data { get; set; } = new();

    public BzChartSeries() { }

    public BzChartSeries(string? label, IEnumerable<double> data, string? color = null)
    {
        Label = label; Data = data.ToList(); Color = color;
    }
}
