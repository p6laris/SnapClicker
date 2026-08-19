namespace SnapClicker.Services;

/// <summary>Tracks mouse movement positions.</summary>
public class MouseTrackerService : IMouseTrackerService, IDisposable
{
    private readonly MouseHook _mouseHook = new();
    
    /// <inheritdoc />
    public event Action<int, int>? OnMouseMove;

    /// <inheritdoc />
    public void StartTracking()
    {
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
        if (action != ActionType.MouseMove)
            OnMouseMove?.Invoke(x, y);
    }

    public void Dispose()
    {
        StopTracking();
        _mouseHook.Dispose();   
    }
}