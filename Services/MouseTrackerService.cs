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
        _mouseHook.OnMouseAction += (x, y, action, _) =>
        {
            if(action != ActionType.MouseMove)
                OnMouseMove?.Invoke(x, y);
        };
        _mouseHook.Start();
    }

    /// <inheritdoc />
    public void StopTracking()
    {
        _mouseHook.Stop();
    }

    public void Dispose()
    {
        StopTracking();
        _mouseHook.Dispose();   
    }
}