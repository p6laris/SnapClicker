namespace SnapClicker.ViewModels.Windows;

public partial class TrackerWindowViewModel : ObservableObject, IDisposable
{
    [ObservableProperty] private bool _isForMouseTracking;
    [ObservableProperty] private int _cursorX;
    [ObservableProperty] private int _cursorY;
    
    private readonly ITrackerManagerService _trackerManagerService;
    private readonly DispatcherTimer _timer;
    
    /// <summary>
    /// On cursor changed event to track mouse position.
    /// </summary>
    public event Action<double,double>? OnCursorPositionChanged;
    public TrackerWindowViewModel(ITrackerManagerService trackerManagerService)
    {
        _trackerManagerService = trackerManagerService;
        _trackerManagerService.OnTrackingModeChanged += TrackerManagerServiceOnOnTrackingModeChanged;
        
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        _timer.Tick += UpdatePosition;
    }

    private void UpdatePosition(object? sender, EventArgs e)
    {
        if (_trackerManagerService.CurrentTrackingMode == TrackingMode.Mouse)
        {
            if (Methods.GetCursorPos(out PointStruct cursorPos))
            {
                if (CursorX != cursorPos.X)
                    CursorX = cursorPos.X;
                if (CursorY != cursorPos.Y)
                    CursorY = cursorPos.Y;
                OnCursorPositionChanged?.Invoke(cursorPos.X, cursorPos.Y);
            }
        }
    }

    private void TrackerManagerServiceOnOnTrackingModeChanged(TrackingMode mode)
    {
        if (mode == TrackingMode.Mouse)
        {
            IsForMouseTracking = true;
            if (!_timer.IsEnabled)
                _timer.Start();
        }
        else if (mode == TrackingMode.Keyboard)
        {
            IsForMouseTracking = false;
            if (_timer.IsEnabled)
                _timer.Stop();
        }
        else
        {
            IsForMouseTracking = true;
            if (_timer.IsEnabled)
                _timer.Stop();
        }
    }

    public void Dispose()
    {
        _timer.Stop();
        _timer.Tick -= UpdatePosition;
        _trackerManagerService.OnTrackingModeChanged -= TrackerManagerServiceOnOnTrackingModeChanged;
    }
}