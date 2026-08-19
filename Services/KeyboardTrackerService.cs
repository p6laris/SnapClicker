namespace SnapClicker.Services;

public class KeyboardTrackerService : IKeyboardTrackerService, IDisposable
{
    private readonly KeyboardHook _keyboardHook = new();
    
    /// <inheritdoc />
    public event Action<Key>? OnKeyDownOrUp;

    /// <inheritdoc />
    public void StartTracking()
    {
        _keyboardHook.OnKeyDown -= HandleKeyDown;
        _keyboardHook.OnKeyDown += HandleKeyDown;
        _keyboardHook.Start();
    }

    /// <inheritdoc />
    public void StopTracking()
    {
        _keyboardHook.OnKeyDown -= HandleKeyDown;
        _keyboardHook.Stop();
    }

    private void HandleKeyDown(Key key, TimeSpan timestamp)
    {
        OnKeyDownOrUp?.Invoke(key);
    }

    public void Dispose()
    {
        StopTracking();
        _keyboardHook.Dispose();
    }
}
