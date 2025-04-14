namespace SnapClicker.Services;

/// <summary>
/// Represents the different tracking modes available for input monitoring.
/// </summary>
public enum TrackingMode
{
    /// <summary>No active tracking</summary>
    None,
    /// <summary>Tracking mouse movements</summary>
    Mouse,
    /// <summary>Tracking keyboard inputs</summary>
    Keyboard
}

/// <summary>
/// Manages input tracking services for both mouse and keyboard, ensuring only one tracking mode is active at a time.
/// </summary>
public class TrackerManagerService : ITrackerManagerService
{
    private readonly IMouseTrackerService _mouseTracker;
    private readonly IKeyboardTrackerService _keyboardTracker;
    
    private Action<int, int>? _mouseCallback;
    private Action<Key>? _keyCallback;
    
    private TrackingMode _currentTrackingMode = TrackingMode.None;
    
    /// <inheritdoc />
    public TrackingMode CurrentTrackingMode => _currentTrackingMode;
    
    /// <inheritdoc />
    public event Action<TrackingMode>? OnTrackingModeChanged;
    
    public TrackerManagerService(IMouseTrackerService mouseTracker, IKeyboardTrackerService keyboardTracker)
    {
        _mouseTracker = mouseTracker;
        _keyboardTracker = keyboardTracker;
    }

    /// <inheritdoc />
    public void StartMouseTracking(Action<int, int> onMouseTracked)
    {
        StopTracking(); // Ensure only one tracking type runs at a time
        _currentTrackingMode = TrackingMode.Mouse;
        _mouseCallback = onMouseTracked;
        _mouseTracker.OnMouseMove += HandleMouseMove;
        _mouseTracker.StartTracking();
        OnTrackingModeChanged?.Invoke(_currentTrackingMode);
    }

    /// <inheritdoc />
    public void StartKeyboardTracking(Action<Key> onKeyTracked)
    {
        StopTracking(); // Ensure only one tracking type runs at a time
        _currentTrackingMode = TrackingMode.Keyboard;
        _keyCallback = onKeyTracked;
        _keyboardTracker.OnKeyDownOrUp += HandleKeyPress;
        _keyboardTracker.StartTracking();
        OnTrackingModeChanged?.Invoke(_currentTrackingMode);
    }

    /// <inheritdoc />
    public void StopTracking()
    {
        if (_currentTrackingMode == TrackingMode.None)
            return;
        
        _mouseTracker.StopTracking();
        _keyboardTracker.StopTracking();
        
        _mouseTracker.OnMouseMove -= HandleMouseMove;
        _keyboardTracker.OnKeyDownOrUp -= HandleKeyPress;

        _mouseCallback = null;
        _keyCallback = null;

        _currentTrackingMode = TrackingMode.None;
        OnTrackingModeChanged?.Invoke(_currentTrackingMode);
    }

    private void HandleMouseMove(int x, int y) => _mouseCallback?.Invoke(x, y);
    private void HandleKeyPress(Key key) => _keyCallback?.Invoke(key);

    /// <inheritdoc />
    public void Dispose()
    { 
        _mouseTracker.OnMouseMove -= HandleMouseMove;
        _keyboardTracker.OnKeyDownOrUp -= HandleKeyPress;
        StopTracking();  
    } 
}