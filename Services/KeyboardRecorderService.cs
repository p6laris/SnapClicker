namespace SnapClicker.Services;

/// <summary>Records keyboard input events.</summary>
public class KeyboardRecorderService : IKeyboardRecorderService, IDisposable
{
    private readonly KeyboardHook _keyboardHook = new();
    
    
    /// <inheritdoc />
    public event Action<RecordedAction>? OnNewKeyRecord;

    /// <inheritdoc />
    public void StartRecording()
    {
        _keyboardHook.OnKeyDown += HandleKeyDown;
        _keyboardHook.OnKeyUp += HandleKeyUp;
        _keyboardHook.Start();
    }


    /// <inheritdoc />
    public void StopRecording()
    {
        _keyboardHook.OnKeyDown -= HandleKeyDown;
        _keyboardHook.OnKeyUp -= HandleKeyUp;
        _keyboardHook.Stop();
    }
    private void HandleKeyDown(Key key, TimeSpan timestamp)
    {
        OnNewKeyRecord?.Invoke(new RecordedAction
        {
            Type = ActionType.KeyDown,
            Key = key,
            Timestamp = timestamp
        });
    }

    private void HandleKeyUp(Key key, TimeSpan timestamp)
    {
        OnNewKeyRecord?.Invoke(new RecordedAction
        {
            Type = ActionType.KeyUp,
            Key = key,
            Timestamp = timestamp
        });
    }

    public void Dispose()
    {
        StopRecording();
        _keyboardHook.Dispose();
    }
}
