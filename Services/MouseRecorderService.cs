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

    private bool _isLeftButtonDown;
    private bool _isRightButtonDown;
    private bool _isMiddleButtonDown;

    private void HandleMouseAction(int x, int y, ActionType actionType, TimeSpan timestamp)
    {
        if (actionType == ActionType.LeftMouseDown) _isLeftButtonDown = true;
        else if (actionType == ActionType.LeftMouseUp) _isLeftButtonDown = false;
        else if (actionType == ActionType.RightMouseDown) _isRightButtonDown = true;
        else if (actionType == ActionType.RightMouseUp) _isRightButtonDown = false;
        else if (actionType == ActionType.MiddleMouseDown) _isMiddleButtonDown = true;
        else if (actionType == ActionType.MiddleMouseUp) _isMiddleButtonDown = false;

        bool isDragging = _isLeftButtonDown || _isRightButtonDown || _isMiddleButtonDown;

        if (actionType == ActionType.MouseMove && !_isMouseMoveRecording && !isDragging)
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