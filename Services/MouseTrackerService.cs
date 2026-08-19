namespace SnapClicker.Services;

/// <summary>Tracks mouse movement positions.</summary>
public class MouseTrackerService : IMouseTrackerService, IDisposable
{
    private static readonly TimeSpan InputCooldown = TimeSpan.FromMilliseconds(300);
    private readonly MouseHook _mouseHook = new();
    private long _startTicks;
    
    /// <inheritdoc />
    public event Action<int, int>? OnMouseMove;

    /// <inheritdoc />
    public void StartTracking()
    {
        _startTicks = Stopwatch.GetTimestamp();
        _mouseHook.OnMouseAction -= HandleMouseAction;
        _mouseHook.OnMouseAction += HandleMouseAction;
        _mouseHook.Start();
    }

    /// <inheritdoc />
    public void StopTracking()
    {
        _mouseHook.OnMouseAction -= HandleMouseAction;
        _mouseHook.Stop();
    }

    private void HandleMouseAction(int x, int y, ActionType action, TimeSpan timestamp)
    {
        if (Stopwatch.GetElapsedTime(_startTicks) < InputCooldown)
            return;

        if (action is ActionType.LeftMouseDown or ActionType.RightMouseDown or ActionType.MiddleMouseDown)
            OnMouseMove?.Invoke(x, y);
    }

    public void Dispose()
    {
        StopTracking();
        _mouseHook.Dispose();   
    }
}