namespace SnapClicker.Services;

public interface IKeyboardTrackerService
{
    
    /// <summary>Occurs when any key is downed or released.</summary>
    event Action<Key>? OnKeyDownOrUp;
    
    /// <summary>Begins monitoring keyboard input.</summary>
    void StartTracking();
    
    /// <summary>Stops keyboard monitoring.</summary>
    void StopTracking();
    
}