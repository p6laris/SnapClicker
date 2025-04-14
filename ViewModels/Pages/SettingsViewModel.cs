using MessageBox = Wpf.Ui.Controls.MessageBox;

namespace SnapClicker.ViewModels.Pages
{
    public partial class SettingsViewModel : ObservableObject, INavigationAware
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
        [ObservableProperty] private double _actionInterval;
        [ObservableProperty] private string _lastCheckedUpdateTime;
        [ObservableProperty] private bool _isUpdateAvailable;
        [ObservableProperty] private string _toUpdateVersion;
        [ObservableProperty] private bool _isProgressing;
        [ObservableProperty] private string _releaseNotesLink;
        [ObservableProperty] private bool _isReleaseNotesLinkAvailable;
        [ObservableProperty] private bool _isWarningFlyoutOpen;
        [ObservableProperty] private string _appVersion = string.Empty;
        [ObservableProperty] private ApplicationTheme _currentTheme = ApplicationThemeManager.GetAppTheme();

        private readonly ObservableList<ThemeOption> _themeOptions = new();
        public NotifyCollectionChangedSynchronizedViewList<ThemeOption> ThemesView { get; }

        public SettingsViewModel(IContentDialogService dialogService, ISnackbarService snackbarService)
        {
            _dialogService = dialogService;
            _snackbarService = snackbarService;
            _updateManager = new UpdateManager(new GithubSource(Source, string.Empty, false));

            ThemesView = _themeOptions.ToNotifyCollectionChangedSlim();
            _themeOptions.AddRange(new[]
            {
                new ThemeOption { DisplayName = "Light", Value = ApplicationTheme.Light },
                new ThemeOption { DisplayName = "Dark", Value = ApplicationTheme.Dark }
            });

            LoadConfig();
        }

        private void LoadConfig()
        {
            StartAndStopBinding = AppConfig.StartAndStopKeyBinding;
            PlayAndStopBinding = AppConfig.PlayAndStopKeyBinding;
            IsMouseMoveRecordingSet = AppConfig.IsMouseMoveRecordingSet;
            IsPreciseDelaySet = AppConfig.IsPreciseDelaysEnabled;
            ActionInterval = AppConfig.ActionInterval;
            LastCheckedUpdateTime = $"Last Checked {AppConfig.LastCheckedUpdate}";
        }

        [RelayCommand]
        public async Task CheckForUpdates()
        {
            try
            {
                IsProgressing = true;
                _updateInfo = await _updateManager.CheckForUpdatesAsync().ConfigureAwait(true);
                if (_updateInfo is null) return;

                var toUpdateVersion = _updateInfo.TargetFullRelease.Version.ToString();

                AppConfig.IsUpdateAvailable = true;
                AppConfig.ToUpdateVersion = toUpdateVersion;
                AppConfig.ReleaseNotesLink = toUpdateVersion;
                AppConfig.IsReleaseNotesAvailable = true;
                AppConfig.LastCheckedUpdate = DateTime.Now;

                IsUpdateAvailable = true;
                ToUpdateVersion = $"SnapClicker v{toUpdateVersion}";
                ReleaseNotesLink = AppConfig.ReleaseNotesLink;
                IsReleaseNotesLinkAvailable = true;
                LastCheckedUpdateTime = $"Last Checked {DateTime.Now}";
            }
            catch (Exception ex)
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
        public async Task DownloadAndInstallUpdates()
        {
            try
            {
                IsProgressing = true;
                if (_updateInfo is null) 
                    return;

                AppConfig.IsUpdateAvailable = false;
                await _updateManager.DownloadUpdatesAsync(_updateInfo).ConfigureAwait(true);
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
            var dialogResult = await new KeyBindingDialog(_dialogService.GetDialogHost()).ShowAsync();
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
            var dialogResult = await new KeyBindingDialog(_dialogService.GetDialogHost()).ShowAsync();
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
            var assembly = System.Reflection.Assembly.GetExecutingAssembly().GetName();
            return $"{assembly.Version.Major}.{assembly.Version.Minor}.{assembly.Version.Build}";
        }
    }
}
