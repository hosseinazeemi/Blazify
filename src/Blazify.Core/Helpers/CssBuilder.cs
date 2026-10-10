using System.Text;

namespace BzCore.Helpers;
public readonly struct CssBuilder
{
    private readonly StringBuilder _sb;

    private CssBuilder(string? initial)
    {
        _sb = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(initial))
            _sb.Append(initial);
    }

    public static CssBuilder Default(string? baseClass = null) => new(baseClass);

    public CssBuilder Add(string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
        {
            if (_sb.Length > 0) _sb.Append(' ');
            _sb.Append(value);
        }
        return this;
    }

    public CssBuilder AddIf(bool condition, string value)
        => condition ? Add(value) : this;

    public CssBuilder AddIf(bool condition, string whenTrue, string whenFalse)
        => Add(condition ? whenTrue : whenFalse);

    public CssBuilder AddFromAttributes(IReadOnlyDictionary<string, object>? attributes)
    {
        if (attributes is not null && attributes.TryGetValue("class", out var c) && c is string s)
            Add(s);
        return this;
    }

    public string Build() => _sb.ToString();
    public override string ToString() => Build();
}

public readonly struct StyleBuilder
{
    private readonly StringBuilder _sb;

    private StyleBuilder(string? initial)
    {
        _sb = new StringBuilder();
        if (!string.IsNullOrWhiteSpace(initial))
            _sb.Append(initial.TrimEnd(';')).Append(';');
    }

    public static StyleBuilder Default(string? initial = null) => new(initial);

    public StyleBuilder Add(string property, string? value)
    {
        if (!string.IsNullOrWhiteSpace(value))
            _sb.Append(property).Append(':').Append(value).Append(';');
        return this;
    }

    public StyleBuilder AddIf(bool condition, string property, string value)
        => condition ? Add(property, value) : this;

    public StyleBuilder AddFromAttributes(IReadOnlyDictionary<string, object>? attributes)
    {
        if (attributes is not null && attributes.TryGetValue("style", out var s) && s is string str)
            _sb.Append(str.TrimEnd(';')).Append(';');
        return this;
    }

    public string Build() => _sb.ToString();
    public override string ToString() => Build();
}
