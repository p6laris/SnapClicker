namespace SnapClicker.Services;

public interface IMouseRecorderService
{
    
    /// <summary>Triggered when mouse actions occur.</summary>
    event Action<RecordedAction>? OnNewMouseRecord;
    
    /// <summary>Starts capturing mouse inputs.</summary>
    void StartRecording();
    
    /// <summary>Stops mouse capture.</summary>
    void StopRecording();
}