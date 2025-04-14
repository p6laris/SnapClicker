using MessageBox = Wpf.Ui.Controls.MessageBox;

namespace SnapClicker.ViewModels.Controls;

public partial class ManualRecordingControlViewModel : ObservableObject, IDisposable
{
    private readonly IContentDialogService _dialogService;
    private readonly IPresetRepository _presetRepository;
    private readonly ITrackerManagerService _trackerManagerService;
    private readonly ISnackbarService _snackbarService;
    private readonly TrackerWindow _trackerWindow;

    [ObservableProperty] private double? _hours;
    [ObservableProperty] private double? _minutes;
    [ObservableProperty] private double? _seconds;
    [ObservableProperty] private double? _milliseconds = 100;
    [ObservableProperty] private ActionType _selectedActionType = ActionType.LeftMouseClick;
    [ObservableProperty] private bool _shouldShowKeySection;
    [ObservableProperty] private int _cursorX;
    [ObservableProperty] private int _cursorY;
    [ObservableProperty] private Key _selectedKey = Key.Enter;
    [ObservableProperty] private bool _isValid = true;

    public IEnumerable<ActionType> ActionTypes => Enum.GetValues(typeof(ActionType))
        .Cast<ActionType>();
    
    public IEnumerable<Key> Keys => Enum.GetValues(typeof(Key)).Cast<Key>();

    public ManualRecordingControlViewModel(
        IPresetRepository presetRepository, 
        IContentDialogService dialogService, 
        ITrackerManagerService trackerManagerService,
        ISnackbarService snackbarService,
        TrackerWindow trackerWindow)
    {
        _dialogService = dialogService;
        _presetRepository = presetRepository;
        _trackerManagerService = trackerManagerService;
        _snackbarService = snackbarService;
        _trackerWindow = trackerWindow;
    }

    partial void OnHoursChanged(double? value) => ValidateTimeFields();
        
    partial void OnMinutesChanged(double? value) => ValidateTimeFields();
        
    partial void OnSecondsChanged(double? value) => ValidateTimeFields();
        
    partial void OnMillisecondsChanged(double? value) => ValidateTimeFields();
     partial void OnSelectedActionTypeChanging(ActionType value) => 
            ShouldShowKeySection = value is ActionType.KeyDown or ActionType.KeyUp;
     
    [RelayCommand]
    public void RecordPosition()
    {
        MinimizeMainWindow();
        _trackerManagerService.StartMouseTracking(OnTrackingMouseCursor);
        _trackerWindow.Show();
    }

    private void OnTrackingMouseCursor(int x, int y)
    {
        CursorX = x;
        CursorY = y;
        StopTracking();
    }

    [RelayCommand]
    public void RecordKey()
    {
        MinimizeMainWindow();
        _trackerManagerService.StartKeyboardTracking(OnTrackingKeyPress);
        CenterRecordWindow();
        _trackerWindow.Show();
    }

    private void OnTrackingKeyPress(Key key)
    {
        SelectedKey = key;
        StopTracking();
    }

    [RelayCommand]
    public async Task SaveRecording()
    {
        var preset = new Preset
        {
            CreatedDate = DateTime.Now,
            RecordedActions =
            {
                new RecordedAction
                {
                    Type = SelectedActionType,
                    Timestamp = new TimeSpan(0, (int)(Hours ?? 0), (int)(Minutes ?? 0), (int)(Seconds ?? 0), (int)(Milliseconds ?? 0)),
                    X = SelectedActionType is ActionType.LeftMouseClick or ActionType.RightMouseClick ? CursorX : 0,
                    Y = SelectedActionType is ActionType.LeftMouseClick or ActionType.RightMouseClick ? CursorY : 0,
                    Key = SelectedActionType is ActionType.LeftMouseClick or ActionType.RightMouseClick ? Key.None : SelectedKey,
                    IsBurstMode = true
                }
            }
        };
        
        await SavePresetAsync(preset);
    }

    private async ValueTask SavePresetAsync(Preset preset)
    {
        var saveDialog = new SaveDialog(_dialogService.GetDialogHost(), preset);
        var result = await saveDialog.ShowAsync();

        if (result != ContentDialogResult.Primary )
            return;

        if (string.IsNullOrEmpty(preset.Name))
        {
            ShowErrorMessage(
                "Preset Name Required", 
                "Please enter a name for your preset before saving.", 
                new SymbolIcon(SymbolRegular.TextboxAlignTop20)
            );
            return;
        }
        
        try
        {
            await _presetRepository.AddPresetAsync(preset);
            WeakReferenceMessenger.Default.Send(new PresetSavedMessage(true));
        }
        catch (Exception e)
        {
            ShowErrorMessage(
                "Save Failed", 
                $"Unable to save preset '{preset.Name}'. Please try again.", 
                new SymbolIcon(SymbolRegular.BookDatabase20)
            );
        }
    }
    
    private void ShowErrorMessage(string title, string content,SymbolIcon icon ) =>
        _snackbarService.Show(title, content,ControlAppearance.Danger, icon, TimeSpan.FromSeconds(5));
    
    private void MinimizeMainWindow()
    {
        if (Application.Current.MainWindow is { } mainWindow)
            mainWindow.WindowState = WindowState.Minimized;
    }

    private void StopTracking()
    {
        _trackerManagerService.StopTracking();
        _trackerWindow.Hide();
        RestoreMainWindow();
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
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ValidateTimeFields() =>
        IsValid = (Hours ?? 0) > 0 || (Minutes ?? 0) > 0 || (Seconds ?? 0) > 0 || (Milliseconds ?? 0) > 0;
    
    public void Dispose() => _trackerManagerService.Dispose();
}
