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
        _trackerManager.StartMouseTracking(TrackMouseCursor);
        _trackerWindow.Show();
    }

    [RelayCommand]
    public void RecordKey()
    {
        MinimizeMainWindow();
        _trackerManager.StartKeyboardTracking(TrackKeyPress);
        CenterRecordWindow();
        _trackerWindow.Show();
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
        _trackerWindow.Hide();
        RestoreMainWindow();
    }

    private void MinimizeMainWindow()
    {
        if (Application.Current.MainWindow is { } mainWindow)
            mainWindow.WindowState = WindowState.Minimized;
    }

    private void RestoreMainWindow()
    {
        if (Application.Current.MainWindow is { } mainWindow)
            mainWindow.WindowState = WindowState.Normal;
    }

    private void CenterRecordWindow()
    {
        _trackerWindow.Left = (SystemParameters.PrimaryScreenWidth - _trackerWindow.Width) / 2;
        _trackerWindow.Top = (SystemParameters.PrimaryScreenHeight - _trackerWindow.Height) / 2;
    }
}
