namespace SnapClicker.Services;

/// <summary>Records mouse movements and clicks.</summary>
public class MouseRecorderService : IMouseRecorderService, IDisposable
{
    private readonly MouseHook _mouseHook = new();
    private bool _isMouseMoveRecording = false;
    
    /// <inheritdoc />
    public event Action<RecordedAction>? OnNewMouseRecord;

    public MouseRecorderService()
    {
        _isMouseMoveRecording = AppConfig.IsMouseMoveRecordingSet;
        WeakReferenceMessenger.Default.Register<MouseMovementRecordingMessage>(this, (r, m) =>
        {
            _isMouseMoveRecording = m.Value;
        });
    }
    
    /// <inheritdoc />
    public void StartRecording()
    {
        _mouseHook.OnMouseAction += HandleMouseAction;
        _mouseHook.Start();
    }

    /// <inheritdoc />
    public void StopRecording()
    {
        _mouseHook.OnMouseAction -= HandleMouseAction;
        _mouseHook.Stop();
    }

    private void HandleMouseAction(int x, int y, ActionType actionType, TimeSpan timestamp)
    {
        if(actionType == ActionType.MouseMove && !_isMouseMoveRecording)
            return;
        
        OnNewMouseRecord?.Invoke(new RecordedAction
        {
            Type = actionType,
            X = x,
            Y = y,
            Timestamp = timestamp,
            Key = Key.None
        });
    }

    public void Dispose()
    {
        StopRecording();
        WeakReferenceMessenger.Default.Unregister<MouseMovementRecordingMessage>(this);
        _mouseHook.Dispose();
    }
}