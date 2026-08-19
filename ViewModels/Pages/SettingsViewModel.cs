using MessageBox = Wpf.Ui.Controls.MessageBox;

namespace SnapClicker.ViewModels.Pages
{
    public partial class SettingsViewModel : ObservableObject, INavigationAware, IDisposable
    {
        private readonly IContentDialogService _dialogService;
        private readonly UpdateManager _updateManager;
        private readonly ISnackbarService _snackbarService;
        
        private UpdateInfo? _updateInfo;
        private bool _isInitialized;

        private const string Source = "https://github.com/p6laris/SnapClicker";

        [ObservableProperty] private KeyBindingModel _startAndStopBinding = new(Key.None, ModifierKeys.None);
        [ObservableProperty] private KeyBindingModel _playAndStopBinding = new(Key.None, ModifierKeys.None);
        [ObservableProperty] private bool _isMouseMoveRecordingSet;
        [ObservableProperty] private bool _isPreciseDelaySet;
        [ObservableProperty] private bool _isTimingJitterEnabled;
        [ObservableProperty] private bool _isTimingJitterExpanded;
        [ObservableProperty] private double _timingJitterRangeMs = 15;
        [ObservableProperty] private bool _isCoordinateJitterEnabled;
        [ObservableProperty] private bool _isCoordinateJitterExpanded;
        [ObservableProperty] private double _coordinateJitterRadiusPx = 3;
        [ObservableProperty] private bool _isCountdownEnabled;
        [ObservableProperty] private bool _isCountdownExpanded;
        [ObservableProperty] private double _countdownSeconds = 3;
        [ObservableProperty] private bool _minimizeToTray = true;
        [ObservableProperty] private bool _closeToTray = true;
        [ObservableProperty] private double _actionInterval;
        [ObservableProperty] private string _selectedPlaybackSpeedOption;
        [ObservableProperty] private string _lastCheckedUpdateTime = string.Empty;
        [ObservableProperty] private bool _isUpdateAvailable;
        [ObservableProperty] private string _toUpdateVersion = string.Empty;
        [ObservableProperty] private bool _isProgressing;
        [ObservableProperty] private string _releaseNotesLink = string.Empty;
        [ObservableProperty] private bool _isReleaseNotesLinkAvailable;
        [ObservableProperty] private string _releaseNotesContent = string.Empty;
        [ObservableProperty] private bool _isReleaseNotesAvailable;
        [ObservableProperty] private int _downloadProgress;
        [ObservableProperty] private bool _isWarningFlyoutOpen;
        [ObservableProperty] private string _appVersion = string.Empty;
        [ObservableProperty] private ApplicationTheme _currentTheme = ApplicationThemeManager.GetAppTheme();

        public List<string> PlaybackSpeedOptions { get; } = new()
        {
            "0.25x", "0.5x", "0.75x", "1x", "1.25x", "1.5x", "2x", "3x", "5x", "10x"
        };

        private readonly ObservableList<ThemeOption> _themeOptions = new();
        public NotifyCollectionChangedSynchronizedViewList<ThemeOption> ThemesView { get; }

        public SettingsViewModel(IContentDialogService dialogService, ISnackbarService snackbarService)
        {
            _dialogService = dialogService;
            _snackbarService = snackbarService;
            _updateManager = new UpdateManager(new GithubSource(Source, string.Empty, false));
            _selectedPlaybackSpeedOption = FormatSpeedOption(AppConfig.PlaybackSpeed);

            ThemesView = _themeOptions.ToNotifyCollectionChanged(SynchronizationContextCollectionEventDispatcher.Current);
            _themeOptions.AddRange(new[]
            {
                new ThemeOption { DisplayName = "Light", Value = ApplicationTheme.Light },
                new ThemeOption { DisplayName = "Dark", Value = ApplicationTheme.Dark }
            });

            WeakReferenceMessenger.Default.Register<PlaybackSpeedMessage>(this, (r, m) =>
            {
                var formatted = FormatSpeedOption(m.Value);
                if (SelectedPlaybackSpeedOption != formatted)
                    SelectedPlaybackSpeedOption = formatted;
            });

            LoadConfig();
        }

        private void LoadConfig()
        {
            StartAndStopBinding = AppConfig.StartAndStopKeyBinding;
            PlayAndStopBinding = AppConfig.PlayAndStopKeyBinding;
            IsMouseMoveRecordingSet = AppConfig.IsMouseMoveRecordingSet;
            IsPreciseDelaySet = AppConfig.IsPreciseDelaysEnabled;
            IsTimingJitterEnabled = AppConfig.IsTimingJitterEnabled;
            IsTimingJitterExpanded = AppConfig.IsTimingJitterEnabled;
            TimingJitterRangeMs = AppConfig.TimingJitterRangeMs;
            IsCoordinateJitterEnabled = AppConfig.IsCoordinateJitterEnabled;
            IsCoordinateJitterExpanded = AppConfig.IsCoordinateJitterEnabled;
            CoordinateJitterRadiusPx = AppConfig.CoordinateJitterRadiusPx;
            IsCountdownEnabled = AppConfig.IsCountdownEnabled;
            IsCountdownExpanded = AppConfig.IsCountdownEnabled;
            CountdownSeconds = AppConfig.CountdownSeconds;
            MinimizeToTray = AppConfig.MinimizeToTray;
            CloseToTray = AppConfig.CloseToTray;
            ActionInterval = AppConfig.ActionInterval;
            SelectedPlaybackSpeedOption = FormatSpeedOption(AppConfig.PlaybackSpeed);
            LastCheckedUpdateTime = $"Last Checked {AppConfig.LastCheckedUpdate}";
            ReleaseNotesLink = "https://github.com/p6laris/SnapClicker/releases";
            IsReleaseNotesLinkAvailable = true;
            IsReleaseNotesAvailable = true;
        }

        [RelayCommand]
        public async Task CheckForUpdates()
        {
            try
            {
                IsProgressing = true;
                _updateInfo = await _updateManager.CheckForUpdatesAsync().ConfigureAwait(true);
                if (_updateInfo?.TargetFullRelease is null)
                {
                    IsUpdateAvailable = false;
                    IsReleaseNotesAvailable = true;
                    IsReleaseNotesLinkAvailable = true;
                    LastCheckedUpdateTime = $"Last Checked {DateTime.Now:g}";
                    return;
                }

                var toUpdateVersion = _updateInfo.TargetFullRelease.Version.ToString();
                var notes = _updateInfo.TargetFullRelease.NotesMarkdown;

                AppConfig.IsUpdateAvailable = true;
                AppConfig.ToUpdateVersion = toUpdateVersion;
                AppConfig.ReleaseNotesLink = toUpdateVersion;
                AppConfig.IsReleaseNotesAvailable = true;
                AppConfig.LastCheckedUpdate = DateTime.Now;

                IsUpdateAvailable = true;
                ToUpdateVersion = $"SnapClicker v{toUpdateVersion}";
                ReleaseNotesContent = !string.IsNullOrWhiteSpace(notes) ? notes : string.Empty;
                IsReleaseNotesAvailable = true;
                ReleaseNotesLink = $"https://github.com/p6laris/SnapClicker/releases/tag/v{toUpdateVersion}";
                IsReleaseNotesLinkAvailable = true;
                LastCheckedUpdateTime = $"Last Checked {DateTime.Now:g}";
            }
            catch (Exception)
            {
                ShowErrorMessage(
                    "Update Check Failed", 
                    "Unable to check for updates. Please check your internet connection and try again.", 
                    new SymbolIcon(SymbolRegular.ArrowDownload20)
                );
            }
            finally
            {
                IsProgressing = false;
            }
        }

        [RelayCommand]
        public async Task ViewReleaseNotesAsync()
        {
            var content = !string.IsNullOrWhiteSpace(ReleaseNotesContent)
                ? ReleaseNotesContent
                : $"You are currently running SnapClicker v1.0.0 (Latest Release).\n\n🚀 Highlights in v1.0.0:\n• Multi-Display & Per-Monitor DPI Awareness\n• Windows System Tray & Context Menu\n• Pre-Start Countdown Overlay & Action Counter\n• Anti-Detection Timing & Coordinate Jitter\n• Playback Speed Multiplier (0.25x - 10x)\n• Escape & Corner Panic Stop Failsafes\n• Zero-Allocation Performance & Fast Loops\n• Drag & Drop Reordering & Multi-Item Actions\n\nFor the full changelog, visit the GitHub Releases page.";

            var dialog = new ContentDialog(_dialogService.GetDialogHostEx())
            {
                Title = !string.IsNullOrWhiteSpace(ToUpdateVersion) ? $"What's New in {ToUpdateVersion}" : "What's New in SnapClicker v1.0.0",
                PrimaryButtonText = "Close",
                DefaultButton = ContentDialogButton.Primary,
                Content = new ScrollViewer
                {
                    MaxHeight = 400,
                    VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                    Content = new Wpf.Ui.Controls.TextBlock
                    {
                        Text = content,
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 13,
                        LineHeight = 20,
                        Margin = new Thickness(4)
                    }
                }
            };

            await dialog.ShowAsync();
        }

        [RelayCommand]
        public async Task DownloadAndInstallUpdates()
        {
            try
            {
                IsProgressing = true;
                if (_updateInfo is null) 
                    return;

                AppConfig.IsUpdateAvailable = false;
                await _updateManager.DownloadUpdatesAsync(_updateInfo, progress =>
                {
                    DownloadProgress = progress;
                }).ConfigureAwait(true);
                _updateManager.ApplyUpdatesAndRestart(_updateInfo);
            }
            catch
            {
                ShowErrorMessage(
                    "Update Installation Failed", 
                    "Failed to install updates. Please ensure you have sufficient permissions and try again.", 
                    new SymbolIcon(SymbolRegular.UninstallApp20)
                );
            }
            finally
            {
                IsProgressing = false;
            }
        }

        partial void OnActionIntervalChanged(double value)
        {
            AppConfig.ActionInterval = value;
            WeakReferenceMessenger.Default.Send(new ActionIntervalMessage(value));
        }

        partial void OnIsMouseMoveRecordingSetChanged(bool value)
        {
            AppConfig.IsMouseMoveRecordingSet = value;
            WeakReferenceMessenger.Default.Send(new MouseMovementRecordingMessage(value));
        }

        partial void OnIsPreciseDelaySetChanged(bool value)
        {
            AppConfig.IsPreciseDelaysEnabled = value;
            WeakReferenceMessenger.Default.Send(new PreciseDelayMessage(value));
        }

        partial void OnIsTimingJitterEnabledChanged(bool value)
        {
            IsTimingJitterExpanded = value;
            AppConfig.IsTimingJitterEnabled = value;
            WeakReferenceMessenger.Default.Send(new TimingJitterMessage((value, (int)TimingJitterRangeMs)));
        }

        partial void OnIsTimingJitterExpandedChanged(bool value)
        {
            if (value && !IsTimingJitterEnabled)
            {
                IsTimingJitterExpanded = false;
            }
        }

        partial void OnTimingJitterRangeMsChanged(double value)
        {
            AppConfig.TimingJitterRangeMs = (int)value;
            WeakReferenceMessenger.Default.Send(new TimingJitterMessage((IsTimingJitterEnabled, (int)value)));
        }

        partial void OnIsCoordinateJitterEnabledChanged(bool value)
        {
            IsCoordinateJitterExpanded = value;
            AppConfig.IsCoordinateJitterEnabled = value;
            WeakReferenceMessenger.Default.Send(new CoordinateJitterMessage((value, (int)CoordinateJitterRadiusPx)));
        }

        partial void OnIsCoordinateJitterExpandedChanged(bool value)
        {
            if (value && !IsCoordinateJitterEnabled)
            {
                IsCoordinateJitterExpanded = false;
            }
        }

        partial void OnCoordinateJitterRadiusPxChanged(double value)
        {
            AppConfig.CoordinateJitterRadiusPx = (int)value;
            WeakReferenceMessenger.Default.Send(new CoordinateJitterMessage((IsCoordinateJitterEnabled, (int)value)));
        }

        partial void OnIsCountdownEnabledChanged(bool value)
        {
            IsCountdownExpanded = value;
            AppConfig.IsCountdownEnabled = value;
        }

        partial void OnIsCountdownExpandedChanged(bool value)
        {
            if (value && !IsCountdownEnabled)
            {
                IsCountdownExpanded = false;
            }
        }

        partial void OnCountdownSecondsChanged(double value)
        {
            AppConfig.CountdownSeconds = (int)value;
        }

        partial void OnMinimizeToTrayChanged(bool value)
        {
            AppConfig.MinimizeToTray = value;
        }

        partial void OnCloseToTrayChanged(bool value)
        {
            AppConfig.CloseToTray = value;
        }

        partial void OnSelectedPlaybackSpeedOptionChanged(string value)
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

        partial void OnCurrentThemeChanged(ApplicationTheme value)
        {
            ApplicationThemeManager.Apply(value);
            AppConfig.Theme = value;
        }

        public Task OnNavigatedToAsync()
        {
            if (!_isInitialized)
            {
                InitializeViewModel();
            }
            InitializeUpdateInfos();
            return Task.CompletedTask;
        }

        public Task OnNavigatedFromAsync() => Task.CompletedTask;

        [RelayCommand]
        public async Task ChangeStartAndStopKeyBinding()
        {
            var dialogResult = await new KeyBindingDialog(_dialogService.GetDialogHostEx()).ShowAsync();
            var viewModel = App.Services.GetRequiredService<KeyBindingDialogViewModel>();

            if (dialogResult != ContentDialogResult.Primary)
            {
                viewModel.Reset();
                return;
            }

            var keyBinding = ExtractKeyBinding(viewModel);
            if (keyBinding.Key == Key.None) return;

            if (!keyBinding.Equals(AppConfig.StartAndStopKeyBinding))
            {
                AppConfig.StartAndStopKeyBinding = keyBinding;
                StartAndStopBinding = keyBinding;
                WeakReferenceMessenger.Default.Send(new StartAndStopRecordHotKeyMessage(keyBinding));
            }
        }

        [RelayCommand]
        public async Task ChangePlayAndStopKeyBinding()
        {
            var dialogResult = await new KeyBindingDialog(_dialogService.GetDialogHostEx()).ShowAsync();
            var viewModel = App.Services.GetRequiredService<KeyBindingDialogViewModel>();

            if (dialogResult != ContentDialogResult.Primary)
            {
                viewModel.Reset();
                return;
            }

            var keyBinding = ExtractKeyBinding(viewModel);
            if (keyBinding.Key == Key.None) return;

            if (!keyBinding.Equals(AppConfig.PlayAndStopKeyBinding))
            {
                AppConfig.PlayAndStopKeyBinding = keyBinding;
                PlayAndStopBinding = keyBinding;
                WeakReferenceMessenger.Default.Send(new PlayAndStopRecordHotKeyMessage(keyBinding));
            }
        }

        [RelayCommand]
        public void ShowWarningFlyout() => IsWarningFlyoutOpen = true;
        private KeyBindingModel ExtractKeyBinding(KeyBindingDialogViewModel viewModel)
        {
            var keys = viewModel.PressedKeys.ToList();
            var modifiers = CombineFlags(keys.Where(IsModifierKey));
            var mainKey = keys.FirstOrDefault(k => !IsModifierKey(k));

            viewModel.Reset();
            return new KeyBindingModel(mainKey, modifiers);
        }

        private void InitializeViewModel()
        {
            ApplicationThemeManager.Apply(CurrentTheme);
            AppVersion = $"SnapClicker v{GetAssemblyVersion()}";
            _isInitialized = true;
        }

        private ModifierKeys CombineFlags(IEnumerable<Key> keys)
        {
            ModifierKeys result = ModifierKeys.None;

            foreach (var key in keys)
            {
                result |= key switch
                {
                    Key.LeftShift or Key.RightShift => ModifierKeys.Shift,
                    Key.LeftCtrl or Key.RightCtrl => ModifierKeys.Control,
                    Key.LeftAlt or Key.RightAlt => ModifierKeys.Alt,
                    Key.LWin or Key.RWin => ModifierKeys.Windows,
                    _ => result
                };
            }

            return result;
        }

        private bool IsModifierKey(Key key) => key is Key.LeftShift or Key.RightShift
                                                   or Key.LeftCtrl or Key.RightCtrl
                                                   or Key.LeftAlt or Key.RightAlt
                                                   or Key.LWin or Key.RWin;

        private void InitializeUpdateInfos()
        {
            IsUpdateAvailable = AppConfig.IsUpdateAvailable;
            IsReleaseNotesLinkAvailable = AppConfig.IsReleaseNotesAvailable;
            ReleaseNotesLink = AppConfig.ReleaseNotesLink;
            ToUpdateVersion = AppConfig.ToUpdateVersion;
        }

        private void ShowErrorMessage(string title, string content,SymbolIcon icon ) =>
            _snackbarService.Show(title, content,ControlAppearance.Danger, icon, TimeSpan.FromSeconds(5));
        
        private string GetAssemblyVersion()
        {
            var version = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version;
            return version != null ? $"{version.Major}.{version.Minor}.{version.Build}" : "1.0.0";
        }

        public void Dispose()
        {
            ThemesView?.Dispose();
        }
    }
}
