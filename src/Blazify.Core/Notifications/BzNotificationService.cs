using BzCore.Enums;

namespace BzCore.Notifications;

public interface IBzNotificationService
{
    event Action? Changed;
    IReadOnlyList<BzNotification> Items { get; }
    BzToastPosition Position { get; set; }

    BzNotification Show(BzNotification notification);
    BzNotification Show(string message, BzSeverity severity = BzSeverity.Info, string? title = null, int duration = 4000, string? group = null);

    BzNotification Success(string message, string? title = null);
    BzNotification Info(string message, string? title = null);
    BzNotification Warning(string message, string? title = null);
    BzNotification Error(string message, string? title = null);

    void ShowGroup(IEnumerable<BzNotification> notifications);

    void Remove(BzNotification notification);
    void Remove(Guid id);
    void Clear();
    void ClearGroup(string group);

    Task<bool> RunAsync(Func<Task> action, BzRunOptions? options = null);
    Task<T?> RunAsync<T>(Func<Task<T>> action, BzRunOptions? options = null);
}

public sealed class BzNotificationService : IBzNotificationService
{
    private readonly List<BzNotification> _items = new();
    private readonly object _gate = new();

    public event Action? Changed;
    public BzToastPosition Position { get; set; } = BzToastPosition.TopEnd;

    public IReadOnlyList<BzNotification> Items
    {
        get { lock (_gate) return _items.ToList(); }
    }

    public BzNotification Show(BzNotification notification)
    {
        lock (_gate) _items.Add(notification);
        Changed?.Invoke();

        if (notification.Duration > 0)
            _ = DismissAfterAsync(notification, notification.Duration);

        return notification;
    }

    public BzNotification Show(string message, BzSeverity severity = BzSeverity.Info, string? title = null, int duration = 4000, string? group = null)
        => Show(new BzNotification { Message = message, Severity = severity, Title = title, Duration = duration, Group = group });

    public BzNotification Success(string message, string? title = null) => Show(message, BzSeverity.Success, title);
    public BzNotification Info(string message, string? title = null) => Show(message, BzSeverity.Info, title);
    public BzNotification Warning(string message, string? title = null) => Show(message, BzSeverity.Warning, title);
    public BzNotification Error(string message, string? title = null) => Show(message, BzSeverity.Error, title, duration: 6000);

    public void ShowGroup(IEnumerable<BzNotification> notifications)
    {
        var list = notifications.ToList();
        lock (_gate) _items.AddRange(list);
        Changed?.Invoke();
        foreach (var n in list.Where(n => n.Duration > 0))
            _ = DismissAfterAsync(n, n.Duration);
    }

    public void Remove(BzNotification notification) => Remove(notification.Id);

    public void Remove(Guid id)
    {
        bool removed;
        lock (_gate) removed = _items.RemoveAll(n => n.Id == id) > 0;
        if (removed) Changed?.Invoke();
    }

    public void Clear()
    {
        lock (_gate) _items.Clear();
        Changed?.Invoke();
    }

    public void ClearGroup(string group)
    {
        bool removed;
        lock (_gate) removed = _items.RemoveAll(n => n.Group == group) > 0;
        if (removed) Changed?.Invoke();
    }

    private async Task DismissAfterAsync(BzNotification n, int ms)
    {
        try { await Task.Delay(ms); } catch { }
        Remove(n);
    }

    public async Task<bool> RunAsync(Func<Task> action, BzRunOptions? options = null)
    {
        options ??= new BzRunOptions();
        var pending = StartPending(options);
        try
        {
            await action();
            if (pending is not null) Remove(pending);
            if (options.ShowSuccess) Show(options.Success, options.SuccessSeverity, duration: options.Duration);
            return true;
        }
        catch (Exception ex)
        {
            if (pending is not null) Remove(pending);
            if (options.ShowError) Show(options.Error ?? ex.Message, options.ErrorSeverity, duration: options.Duration);
            return false;
        }
    }

    public async Task<T?> RunAsync<T>(Func<Task<T>> action, BzRunOptions? options = null)
    {
        options ??= new BzRunOptions();
        var pending = StartPending(options);
        try
        {
            var result = await action();
            if (pending is not null) Remove(pending);
            if (options.ShowSuccess) Show(options.Success, options.SuccessSeverity, duration: options.Duration);
            return result;
        }
        catch (Exception ex)
        {
            if (pending is not null) Remove(pending);
            if (options.ShowError) Show(options.Error ?? ex.Message, options.ErrorSeverity, duration: options.Duration);
            return default;
        }
    }

    private BzNotification? StartPending(BzRunOptions o)
        => o.Pending is null ? null : Show(new BzNotification { Message = o.Pending, Severity = BzSeverity.Info, Duration = 0, Closable = false, ShowProgress = false });
}
