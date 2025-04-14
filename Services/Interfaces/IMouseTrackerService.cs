namespace SnapClicker.Services;
public interface IMouseTrackerService
{
    /// <summary>Raised when mouse moves.</summary>
    event Action<int, int>? OnMouseMove;
    
    /// <summary>Stops mouse tracking.</summary>
    void StartTracking();
    
    /// <summary>Cleans up tracking resources.</summary>
    void StopTracking();
}