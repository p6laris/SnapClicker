namespace SnapClicker.ViewModels.Pages
{
    public partial class RecordPageViewModel : ObservableObject, IDisposable
    {
        private readonly IInputSimulatorService _inputSimulatorService;
        private readonly IHotKeyManager _hotKeyManager;
        private readonly INavigationView _navigationView;
        
        private int _hotkeyId;
        private CancellationTokenSource? _cancellationTokenSource;
        
        [ObservableProperty] private PresetsDto? _selectedPreset;
        [ObservableProperty] private bool _isSimulationRunning;
        [ObservableProperty] private string _selectedSpeedOption;

        public List<string> SpeedOptions { get; } = new()
        {
            "0.25x", "0.5x", "0.75x", "1x", "1.25x", "1.5x", "2x", "3x", "5x", "10x"
        };

        public RecordPageViewModel(
            IInputSimulatorService inputSimulatorService,
            INavigationService navigationService,
            IHotKeyManager hotKeyManager)
        {
            _inputSimulatorService = inputSimulatorService;
            _hotKeyManager = hotKeyManager;
            _navigationView = navigationService.GetNavigationControl();
            _selectedSpeedOption = FormatSpeedOption(AppConfig.PlaybackSpeed);

            RegisterHotKeys();
            WeakReferenceMessenger.Default.Register<PlayAndStopRecordHotKeyMessage>(this, (r, m) =>
            {
                UpdateHotKey(m.Value);
            });

            WeakReferenceMessenger.Default.Register<PlaybackSpeedMessage>(this, (r, m) =>
            {
                var formatted = FormatSpeedOption(m.Value);
                if (SelectedSpeedOption != formatted)
                    SelectedSpeedOption = formatted;
            });
        }

        partial void OnSelectedSpeedOptionChanged(string value)
        {
            if (string.IsNullOrWhiteSpace(value)) return;
            var clean = value.TrimEnd('x', 'X').Trim();
            if (double.TryParse(clean, NumberStyles.Any, CultureInfo.InvariantCulture, out var speed) && speed > 0)
            {
                AppConfig.PlaybackSpeed = speed;
                WeakReferenceMessenger.Default.Send(new PlaybackSpeedMessage(speed));
            }
        }

        private static string FormatSpeedOption(double speed)
        {
            return speed % 1 == 0 ? $"{speed:0}x" : $"{speed:0.##}x";
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
            try
            {
                // If simulation is currently running, ALWAYS stop immediately!
                if (IsSimulationRunning)
                {
                    StopActions();
                    return;
                }

                var isRecordPageActive = _navigationView.SelectedItem?.Content is string content && content == "Record";
                if (!isRecordPageActive)
                    return;

                if (SelectedPreset != null)
                {
                    await PlayActionsAsync();
                }
            }
            catch (Exception ex)
            {
                Log.Error(ex, "Error in playback hotkey handler.");
            }
        }

        [RelayCommand]
        public async Task PlayActionsAsync()
        {
            if (IsSimulationRunning)
                return;

            if (SelectedPreset?.RecordedActions is { Count: > 0 } rawActions)
            {
                var sortedActions = rawActions.OrderBy(x => x.Timestamp).ToList();
                IsSimulationRunning = true;

                // Cancel and dispose previous token source if any
                _cancellationTokenSource?.Cancel();
                _cancellationTokenSource?.Dispose();
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

                                await _inputSimulatorService.Simulate(sortedActions, _cancellationTokenSource.Token);
                            }
                        }
                        else
                        {
                            while (!_cancellationTokenSource.Token.IsCancellationRequested)
                            {
                                await _inputSimulatorService.Simulate(sortedActions, _cancellationTokenSource.Token);
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
            {
                if (state == WindowState.Normal && mainWindow is MainWindow mw)
                {
                    mw.RestoreFromTray();
                }
                else
                {
                    mainWindow.WindowState = state;
                }
            }
        }
        
        public void Dispose()
        {
            _cancellationTokenSource?.Cancel();
            _cancellationTokenSource?.Dispose();
            
            _hotKeyManager.UnregisterHotKey(_hotkeyId);
            WeakReferenceMessenger.Default.Unregister<PlayAndStopRecordHotKeyMessage>(this);
            WeakReferenceMessenger.Default.Unregister<PlaybackSpeedMessage>(this);
        }
    }
}
