namespace SnapClicker.Services;

public interface IRecorderManagerService : IDisposable
{
    /// <summary>
    /// Occurs when either mouse or keyboard input is recorded.
    /// </summary>
    event Action<RecordedAction>? OnNewRecord;
    
    /// <summary>
    /// Begins simultaneous recording of both mouse and keyboard inputs.
    /// </summary>
    void StartRecording();
    
    /// <summary>
    /// Stops active recording from both input devices.
    /// </summary>
    void StopRecording();
}