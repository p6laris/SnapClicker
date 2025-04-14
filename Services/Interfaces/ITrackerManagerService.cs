namespace SnapClicker.Services;

public interface ITrackerManagerService : IDisposable
{
    /// <summary>
    /// Occurs when the tracking mode changes.
    /// </summary>
    event Action<TrackingMode>? OnTrackingModeChanged;
    
    /// <summary>
    /// Gets the current active tracking mode.
    /// </summary>
    TrackingMode CurrentTrackingMode { get; }
    
    /// <summary>
    /// Starts tracking mouse movements and invokes the callback when the mouse moves.
    /// </summary>
    /// <param name="onMouseTracked">The callback to invoke when mouse movement is detected.</param>
    /// <remarks>
    /// This will automatically stop any existing tracking before starting mouse tracking.
    /// </remarks>
    void StartMouseTracking(Action<int, int> onMouseTracked);
    
    /// <summary>
    /// Starts tracking keyboard inputs and invokes the callback when a key is pressed.
    /// </summary>
    /// <param name="onKeyTracked">The callback to invoke when a key press is detected.</param>
    /// <remarks>
    /// This will automatically stop any existing tracking before starting keyboard tracking.
    /// </remarks>
    void StartKeyboardTracking(Action<Key> onKeyTracked);
    
    /// <summary>
    /// Stops all active tracking and resets to <see cref="TrackingMode.None"/>.
    /// </summary>
    void StopTracking();
}