namespace SnapClicker.Services;

public class KeyboardTrackerService : IKeyboardTrackerService, IDisposable
{
    private readonly KeyboardHook _keyboardHook = new();
    
    /// <inheritdoc />
    public event Action<Key>? OnKeyDownOrUp;

    /// <inheritdoc />
    public void StartTracking()
    {
        _keyboardHook.OnKeyDown += (key, _) => OnKeyDownOrUp?.Invoke(key);
        _keyboardHook.Start();
    }

    /// <inheritdoc />
    public void StopTracking()
    {
        _keyboardHook.Stop();
    }

    public void Dispose()
    {
        StopTracking();
        _keyboardHook.Dispose();
    }
}
