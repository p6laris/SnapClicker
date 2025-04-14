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
        _timer.Start();
    }

    private void UpdatePosition(object? sender, EventArgs e)
    {
        //If it was for tracking mouse then we need to change the window location
        if (_trackerManagerService.CurrentTrackingMode == TrackingMode.Mouse)
        {
            if (GetCursorPos(out POINT cursorPos))
            {
                CursorX = cursorPos.X;
                CursorY = cursorPos.Y;
                OnCursorPositionChanged?.Invoke(cursorPos.X, cursorPos.Y);
            }
        }

    }

    private void TrackerManagerServiceOnOnTrackingModeChanged(TrackingMode mode)
    {
        if(mode == TrackingMode.Mouse || mode == TrackingMode.None)
            IsForMouseTracking = true;
        else if(mode == TrackingMode.Keyboard)
            IsForMouseTracking = false;
    }
    
    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);
    
    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    public void Dispose()
    {
        _trackerManagerService.OnTrackingModeChanged -= TrackerManagerServiceOnOnTrackingModeChanged;
       _trackerManagerService.Dispose();
    }
}