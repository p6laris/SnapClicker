namespace SnapClicker.Services;

/// <summary>
/// Coordinates recording of both mouse and keyboard inputs, merging them into a single stream of recorded actions.
/// </summary>
public class RecorderManagerService : IRecorderManagerService
{
    private readonly IMouseRecorderService _mouseRecorder;
    private readonly IKeyboardRecorderService _keyboardRecorder;
    
    /// <inheritdoc />
    public event Action<RecordedAction>? OnNewRecord;
    
    public RecorderManagerService(IMouseRecorderService mouseRecorder, IKeyboardRecorderService keyboardRecorder)
    {
        _mouseRecorder = mouseRecorder;
        _keyboardRecorder = keyboardRecorder;
        
        _mouseRecorder.OnNewMouseRecord += RaiseNewRecord;
        _keyboardRecorder.OnNewKeyRecord += RaiseNewRecord;
    }
    
    /// <inheritdoc />
    public void StartRecording()
    {
        _mouseRecorder.StartRecording();
        _keyboardRecorder.StartRecording();
    }
    
    /// <inheritdoc />
    public void StopRecording()
    {
        _mouseRecorder.StopRecording();
        _keyboardRecorder.StopRecording();
    }

    private void RaiseNewRecord(RecordedAction action)
    {
        OnNewRecord?.Invoke(action);
    }

    public void Dispose()
    {
        _mouseRecorder.OnNewMouseRecord -= RaiseNewRecord;
        _keyboardRecorder.OnNewKeyRecord -= RaiseNewRecord;
        StopRecording();
    }
}