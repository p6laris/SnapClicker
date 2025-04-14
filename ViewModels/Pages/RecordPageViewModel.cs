namespace SnapClicker.ViewModels.Pages
{
    public partial class RecordPageViewModel : ObservableObject, IDisposable
    {
        private readonly IInputSimulatorService _inputSimulatorService;
        private readonly IHotKeyManager _hotKeyManager;
        private readonly INavigationView _navigationView;
        
        private int _hotkeyId;
        private CancellationTokenSource _cancellationTokenSource;
        
        [ObservableProperty] private PresetsDto _selectedPreset;
        [ObservableProperty] private bool _isSimulationRunning;

        public RecordPageViewModel(
            IInputSimulatorService inputSimulatorService,
            INavigationService navigationService,
            IHotKeyManager hotKeyManager)
        {
            _inputSimulatorService = inputSimulatorService;
            _hotKeyManager = hotKeyManager;
            _navigationView = navigationService.GetNavigationControl();

            RegisterHotKeys();
            WeakReferenceMessenger.Default.Register<PlayAndStopRecordHotKeyMessage>(this, (r, m) =>
            {
                UpdateHotKey(m.Value);
            });
            
        }

        private void RegisterHotKeys()
        {
            var keyBinding = AppConfig.PlayAndStopKeyBinding;
            
            _hotkeyId = _hotKeyManager.RegisterHotKey(
                keyBinding.Key, keyBinding.ModifierKeys,
                OnStartOrStop);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void UpdateHotKey(KeyBindingModel keyBinding) 
            => _hotKeyManager.UpdateHotKey(_hotkeyId, keyBinding.Key, keyBinding.ModifierKeys);
        
        private async void OnStartOrStop()
        {
            //Only trigger the hotkeys when we are in record page.
            var isRecordPageActive = (string)_navigationView.SelectedItem.Content == "Record";
            if (!isRecordPageActive)
                return;
            
            if (IsSimulationRunning)
                StopActions();
            else
                await PlayActionsAsync();
        }

        [RelayCommand]
        public async Task PlayActionsAsync()
        {
            if (SelectedPreset?.RecordedActions?.Count > 0)
            {
                IsSimulationRunning = true;
                _cancellationTokenSource = new CancellationTokenSource();

                try
                {
                    ChangeWindowsState(WindowState.Minimized);

                    await Task.Run(async () =>
                    {
                        if (SelectedPreset.IsRepetitive)
                        {
                            for (int i = 0; i < SelectedPreset.RepeatCount; i++)
                            {
                                if (_cancellationTokenSource.Token.IsCancellationRequested)
                                    break;

                                await SimulateAsync(_cancellationTokenSource.Token);
                            }
                        }
                        else
                        {
                            while (!_cancellationTokenSource.Token.IsCancellationRequested)
                            {
                                await SimulateAsync(_cancellationTokenSource.Token);
                            }
                        }
                    });
                }
                catch (TaskCanceledException)
                {
                    // expected on cancel
                }
                finally
                {
                    IsSimulationRunning = false;
                    ChangeWindowsState(WindowState.Normal);
                }
            }
        }
        
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private async ValueTask SimulateAsync(CancellationToken cancellationToken) => 
            await _inputSimulatorService.Simulate(SelectedPreset.RecordedActions.OrderBy(x => x.Timestamp).ToList(), cancellationToken);
        

        [RelayCommand]
        public void StopActions()
        {
            _cancellationTokenSource?.Cancel();
            IsSimulationRunning = false;
        }
        private void ChangeWindowsState(WindowState state)
        {
            var mainWindow = Application.Current.MainWindow;
            if (mainWindow != null)
                mainWindow.WindowState = state;
        }
        
        public void Dispose()
        {
            if(_cancellationTokenSource is not null)
                _cancellationTokenSource.Dispose();
            
            _hotKeyManager.UnregisterHotKey(_hotkeyId);
            WeakReferenceMessenger.Default.Unregister<PlayAndStopRecordHotKeyMessage>(this);
        }
    }
}
