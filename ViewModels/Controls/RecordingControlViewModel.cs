namespace SnapClicker.ViewModels.Controls;

public partial class RecordingControlViewModel : ObservableObject, IDisposable
{ 
    private readonly IContentDialogService _contentDialogService;
    private readonly IRecorderManagerService _recorderManagerService;
    private readonly IPresetRepository _presetRepository;
    private readonly RecordWindow _recordWindow;
    private readonly IHotKeyManager _hotKeyManager;
    private readonly ISnackbarService _snackbarService;
    private readonly INavigationView _navigationView;
    
    private int _hotkeyId;
    
    [ObservableProperty] private bool _hasRecords;
    [ObservableProperty] private bool _isRecording;

    private ObservableList<RecordedAction> _recordedList = new();
    public NotifyCollectionChangedSynchronizedViewList<RecordedAction> RecordsView { get; set; }
    
    public RecordingControlViewModel(
        IContentDialogService contentDialogService, 
        IRecorderManagerService recorderManagerService,
        IPresetRepository presetRepository,
        INavigationService navigationService,
        IHotKeyManager hotKeyManager,
        ISnackbarService snackbarService,
        RecordWindow recordWindow)
    {
        _contentDialogService = contentDialogService;
        _recorderManagerService = recorderManagerService;
        _presetRepository = presetRepository;
        _navigationView = navigationService.GetNavigationControl();
        _hotKeyManager = hotKeyManager;
        _snackbarService = snackbarService;
        _recordWindow = recordWindow;

        RecordsView =
            _recordedList.ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);
        
        _recorderManagerService.OnNewRecord += AddNewRecord;
        RegisterHotKeys();
        
        //When recording hot key modified.
        WeakReferenceMessenger.Default.Register<StartAndStopRecordHotKeyMessage>(this, (r, m) =>
        {
            UpdateHotkey(m.Value);
        });
    }

    private void RegisterHotKeys()
    {
        var keyBinding = AppConfig.StartAndStopKeyBinding;
        
        _hotkeyId = _hotKeyManager.RegisterHotKey(
            keyBinding.Key, 
            keyBinding.ModifierKeys,
            OnStartOrRecordingHotkey);
    }

    private void UpdateHotkey(KeyBindingModel keyBinding)
        => _hotKeyManager.UpdateHotKey(_hotkeyId, keyBinding.Key, keyBinding.ModifierKeys);

    private void OnStartOrRecordingHotkey()
    {
        //Only trigger the hotkey when we are in SnapClicker page(Recording page)
        var isDashboardPageIsActive = (string)_navigationView.SelectedItem.Content == "SnapClicker";
        if (!isDashboardPageIsActive)
            return;
        
        (IsRecording ? StopRecordingCommand : StartRecordingCommand).Execute(null);   
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void AddNewRecord(RecordedAction action)
    {
        _recordedList.Add(action);
        HasRecords = _recordedList.Any();
    }
    
    [RelayCommand]
    public async Task StartRecording()
    {
        if (_recordedList.Any())
            ClearRecords();

        _recordWindow.Show();
        SetCursorPositionToCenter();

        HasRecords = false;
        IsRecording = true;

        ChangeWindowsState(WindowState.Minimized);
        
        await Task.Delay(500);
        _recorderManagerService.StartRecording();
    }

    [RelayCommand]
    public void StopRecording()
    {
        IsRecording = false;
        if(HasRecords)
            DeleteRecordsByHotKey();
        
        HasRecords = _recordedList.Any();
        _recordWindow.Hide();
        _recorderManagerService.StopRecording();   
        
        ChangeWindowsState(WindowState.Normal);
    }

    private void DeleteRecordsByHotKey() 
    {
        //Delete key inputs recorded by the input recorder
        var hotKey = _hotKeyManager.GetHotKeyById(_hotkeyId);
        if (hotKey is null || _recordedList.Count == 0)
            return;
        
        var (key, modifiers) = hotKey.Value;
        int keyCount = (modifiers.BitCount() + 1);
        
        if (_recordedList.Count < keyCount) 
            return; 

        var endKeys = _recordedList.Skip(_recordedList.Count - keyCount).Take(keyCount).Select(a => a.Key).ToList();

        var expectedKeys = GetExpectedKeys(modifiers, key);
        bool matchesEnd = new HashSet<Key>(endKeys).SetEquals(expectedKeys);

        if (matchesEnd)
            _recordedList.RemoveRange(_recordedList.Count - keyCount, keyCount);
    }

    private static HashSet<Key> GetExpectedKeys(ModifierKeys modifiers, Key key)
    {
        var expectedKeys = new HashSet<Key>(); // Use HashSet for unordered comparison
        if (modifiers.HasFlag(ModifierKeys.Control)) expectedKeys.Add(Key.LeftCtrl);
        if (modifiers.HasFlag(ModifierKeys.Shift)) expectedKeys.Add(Key.LeftShift);
        if (modifiers.HasFlag(ModifierKeys.Alt)) expectedKeys.Add(Key.LeftAlt);
        if (modifiers.HasFlag(ModifierKeys.Windows)) expectedKeys.Add(Key.LWin);
        expectedKeys.Add(key);
        
        return expectedKeys;
    }
    [RelayCommand]
    private void ClearRecords()
    {
        _recordedList.Clear();
        HasRecords = false; 
    }

    [RelayCommand]
    private async Task SaveRecords() 
        => await SavePreset(new Preset { RecordedActions = _recordedList.ToList(), CreatedDate = DateTime.Now });
    
    private async ValueTask SavePreset(Preset preset)
    {
        var saveDialog = new SaveDialog(_contentDialogService.GetDialogHost(), preset);
        if (await saveDialog.ShowAsync() != ContentDialogResult.Primary ) 
            return;
        
        if(string.IsNullOrEmpty(preset.Name))
            ShowErrorMessage("Missing Name", "Please try again and enter a name for your preset.", new SymbolIcon(SymbolRegular.BookInformation20));
        try
        {
            await _presetRepository.AddPresetAsync(preset);
            WeakReferenceMessenger.Default.Send(new PresetSavedMessage(true));
        }
        catch (Exception e)
        {
            ShowErrorMessage("Save Failed", $"Couldn't save '{preset.Name}'.", new SymbolIcon(SymbolRegular.BookDatabase20));
        }
    }
    
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private void ChangeWindowsState(WindowState state) 
        => Application.Current.MainWindow?.SetCurrentValue(Window.WindowStateProperty, state);

    private void ShowErrorMessage(string title, string content,SymbolIcon icon ) =>
        _snackbarService.Show(title, content,ControlAppearance.Danger, icon, TimeSpan.FromSeconds(5));
    
    private void SetCursorPositionToCenter()
    {
        var (screenWidth, screenHeight) = (SystemParameters.VirtualScreenWidth, SystemParameters.VirtualScreenHeight);
        Methods.SetCursorPos((int)(screenWidth / 2), (int)(screenHeight / 2));
    }

    public void Dispose()
    {
        WeakReferenceMessenger.Default.Unregister<StartAndStopRecordHotKeyMessage>(this);
        RecordsView.Dispose();
        _hotKeyManager.UnregisterHotKey(_hotkeyId);
    }
}