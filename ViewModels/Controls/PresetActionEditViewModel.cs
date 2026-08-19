namespace SnapClicker.ViewModels.Controls;

public partial class PresetActionEditViewModel : ObservableObject
{
    private readonly ITrackerManagerService _trackerManager;
    private readonly TrackerWindow _trackerWindow;

    [ObservableProperty] private int _cursorX;
    [ObservableProperty] private int _cursorY;
    [ObservableProperty] private Key _key;
    [ObservableProperty] private ActionType _actionType;
    [ObservableProperty] private bool _isMouseAction = true;

    public IEnumerable<Key> Keys => Enum.GetValues(typeof(Key)).Cast<Key>();
    public IEnumerable<ActionType> ActionTypes => Enum.GetValues(typeof(ActionType)).Cast<ActionType>();

    public PresetActionEditViewModel(ITrackerManagerService trackerManagerService, TrackerWindow trackerWindow)
    {
        _trackerManager = trackerManagerService;
        _trackerWindow = trackerWindow;
    }

    [RelayCommand]
    public void RecordPosition()
    {
        MinimizeMainWindow();
        _trackerWindow.Show();
        _trackerManager.StartMouseTracking(TrackMouseCursor);
    }

    [RelayCommand]
    public void RecordKey()
    {
        MinimizeMainWindow();
        CenterRecordWindow();
        _trackerWindow.Show();
        _trackerManager.StartKeyboardTracking(TrackKeyPress);
    }

    private void TrackMouseCursor(int x, int y)
    {
        CursorX = x;
        CursorY = y;
        StopTrackingAndRestore();
    }

    private void TrackKeyPress(Key key)
    {
        Key = key;
        StopTrackingAndRestore();
    }

    private void StopTrackingAndRestore()
    {
        _trackerManager.StopTracking();
        Application.Current.Dispatcher.Invoke(() =>
        {
            _trackerWindow.Hide();
            RestoreMainWindow();
        });
    }

    private void MinimizeMainWindow()
    {
        Application.Current.Dispatcher.Invoke(() =>
        {
            if (Application.Current.MainWindow is { } mainWindow)
                mainWindow.WindowState = WindowState.Minimized;
        });
    }

    private void RestoreMainWindow()
    {
        if (Application.Current.MainWindow is MainWindow mainWindow)
        {
            mainWindow.RestoreFromTray();
        }
    }

    private void CenterRecordWindow()
    {
        _trackerWindow.Left = (SystemParameters.PrimaryScreenWidth - _trackerWindow.Width) / 2;
        _trackerWindow.Top = (SystemParameters.PrimaryScreenHeight - _trackerWindow.Height) / 2;
    }
}
