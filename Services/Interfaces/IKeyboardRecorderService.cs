namespace SnapClicker.Services;

public interface IKeyboardRecorderService
{
    
    /// <summary>Triggered when a key is downed or released.</summary>
    event Action<RecordedAction>? OnNewKeyRecord;
    
    /// <summary>Starts capturing keyboard inputs.</summary>
    void StartRecording();
    
    /// <summary>Stops keyboard capture.</summary>
    void StopRecording();
    
}