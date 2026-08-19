namespace SnapClicker.Services;

public class KeyboardTrackerService : IKeyboardTrackerService, IDisposable
{
    private static readonly TimeSpan InputCooldown = TimeSpan.FromMilliseconds(300);
    private readonly KeyboardHook _keyboardHook = new();
    private long _startTicks;
    
    /// <inheritdoc />
    public event Action<Key>? OnKeyDownOrUp;

    /// <inheritdoc />
    public void StartTracking()
    {
        _startTicks = Stopwatch.GetTimestamp();
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
        if (Stopwatch.GetElapsedTime(_startTicks) < InputCooldown)
            return;

        OnKeyDownOrUp?.Invoke(key);
    }

    public void Dispose()
    {
        StopTracking();
        _keyboardHook.Dispose();
    }
}
