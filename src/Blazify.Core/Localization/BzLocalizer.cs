using BzCore.Enums;
using BzCore.Theming;

namespace BzCore.Localization;

public class BzLocalizer : IBzLocalizer
{
    private readonly Dictionary<BzLanguage, Dictionary<string, string>> _res = new();
    private readonly IBzThemeService? _theme;

    public BzLocalizer(IBzThemeService? theme = null)
    {
        _theme = theme;
        Seed();
    }

    public BzLanguage Language { get; private set; } = BzLanguage.Fa;
    public bool IsRtl => Language is BzLanguage.Fa;
    public event Action? Changed;

    public string this[string key] => Lookup(key);

    public string T(string key, params object[] args)
    {
        var s = Lookup(key);
        return args.Length > 0 ? string.Format(s, args) : s;
    }

    public void SetLanguage(BzLanguage lang)
    {
        Language = lang;
        _theme?.SetDirection(IsRtl ? BzDirection.Rtl : BzDirection.Ltr);
        Changed?.Invoke();
    }

    public void AddResources(BzLanguage lang, IDictionary<string, string> entries)
    {
        if (!_res.TryGetValue(lang, out var d)) { d = new(); _res[lang] = d; }
        foreach (var kv in entries) d[kv.Key] = kv.Value;
    }

    private string Lookup(string key)
    {
        if (_res.TryGetValue(Language, out var d) && d.TryGetValue(key, out var v)) return v;
        if (_res.TryGetValue(BzLanguage.Fa, out var fa) && fa.TryGetValue(key, out var f)) return f;
        return key;
    }

    private void Seed()
    {
        AddResources(BzLanguage.Fa, new Dictionary<string, string>
        {
            ["save"] = "ذخیره",
            ["cancel"] = "انصراف",
            ["search"] = "جستجو",
            ["welcome"] = "خوش آمدید",
            ["hello_name"] = "سلام، {0}!",
            ["language"] = "زبان",
            ["yes"] = "بله",
            ["no"] = "خیر",
            ["settings"] = "تنظیمات",
        });
        AddResources(BzLanguage.En, new Dictionary<string, string>
        {
            ["save"] = "Save",
            ["cancel"] = "Cancel",
            ["search"] = "Search",
            ["welcome"] = "Welcome",
            ["hello_name"] = "Hello, {0}!",
            ["language"] = "Language",
            ["yes"] = "Yes",
            ["no"] = "No",
            ["settings"] = "Settings",
        });
    }
}
