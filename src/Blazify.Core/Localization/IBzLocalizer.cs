namespace BzCore.Localization;

public interface IBzLocalizer
{
    BzLanguage Language { get; }
    bool IsRtl { get; }
    event Action? Changed;

    string this[string key] { get; }
    string T(string key, params object[] args);

    void SetLanguage(BzLanguage lang);
    void AddResources(BzLanguage lang, IDictionary<string, string> entries);
}
