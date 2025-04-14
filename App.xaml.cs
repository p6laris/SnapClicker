namespace SnapClicker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        // The.NET Generic Host provides dependency injection, configuration, logging, and other services.
        // https://docs.microsoft.com/dotnet/core/extensions/generic-host
        // https://docs.microsoft.com/dotnet/core/extensions/dependency-injection
        // https://docs.microsoft.com/dotnet/core/extensions/configuration
        // https://docs.microsoft.com/dotnet/core/extensions/logging
        private static readonly IHost _host = Host
            .CreateDefaultBuilder()
            .ConfigureAppConfiguration(c => { c.SetBasePath(Path.GetDirectoryName(AppContext.BaseDirectory)); })
            .ConfigureServices((context, services) =>
            {
                //Depenedency Injection
                services.AddNavigationViewPageProvider();

                // Page resolver service
                services.AddHostedService<ApplicationHostService>();

                // Theme manipulation
                services.AddSingleton<IThemeService, ThemeService>();

                // TaskBar manipulation
                services.AddSingleton<ITaskBarService, TaskBarService>();

                // Service containing navigation, same as INavigationWindow... but without window
                services.AddSingleton<INavigationService, NavigationService>();
                
                //Tracker and recorder services
                services.AddSingleton<ITrackerManagerService, TrackerManagerService>();
                services.AddSingleton<IMouseTrackerService, MouseTrackerService>();
                services.AddSingleton<IKeyboardTrackerService, KeyboardTrackerService>();
                services.AddSingleton<IMouseRecorderService,MouseRecorderService>();
                services.AddSingleton<IKeyboardRecorderService, KeyboardRecorderService>();
                services.AddSingleton<IRecorderManagerService, RecorderManagerService>();
                services.AddSingleton<TrackerWindow>();
                services.AddSingleton<RecordWindow>();
                services.AddTransient<WindowPositionService>();
                //Simulator services
                services.AddSingleton<IInputSimulatorService, InputSimulatorService>();
                // Content dialog service
                services.AddSingleton<IContentDialogService, ContentDialogService>();
                // Main window with navigation
                services.AddSingleton<INavigationWindow, MainWindow>();
                services.AddSingleton<MainWindowViewModel>();
                
                // View models
                services.AddSingleton<DashboardPage>();
                services.AddSingleton<DashboardViewModel>();
                services.AddSingleton<SettingsPage>();
                services.AddSingleton<SettingsViewModel>();
                services.AddSingleton<TrackerWindowViewModel>();
                services.AddSingleton<ManualRecordingControlViewModel>();
                services.AddSingleton<PresetActionEditViewModel>();
                services.AddSingleton<RecordWindowViewModel>();
                services.AddSingleton<RecordingControlViewModel>();
                services.AddSingleton<PresetsControlViewModel>();
                services.AddSingleton<RecordPage>();
                services.AddSingleton<RecordPageViewModel>();
                
                //Repositories services
                services.AddSingleton<SnapClickerDbContext>();
                services.AddSingleton<IPresetRepository, PresetRepository>();
                services.AddSingleton<IHotKeyManager, HotKeyManager>();
                services.AddSingleton<KeyBindingDialogViewModel>();

                services.AddSingleton<ISnackbarService, SnackbarService>();
            }).Build();

        /// <summary>
        /// Gets services.
        /// </summary>
        public static IServiceProvider Services => _host.Services;

        /// <summary>
        /// Occurs when the application is loading.
        /// </summary>
        private async void OnStartup(object sender, StartupEventArgs e)
        {
            VelopackApp.Build().Run();
            
            await SetDatabase();
            await _host.StartAsync();
        }

        private async ValueTask SetDatabase()
        {
            DatabaseFacade facade = new DatabaseFacade(new SnapClickerDbContext());
            await facade.EnsureCreatedAsync();
        }

        /// <summary>
        /// Occurs when the application is closing.
        /// </summary>
        private async void OnExit(object sender, ExitEventArgs e)
        {
            await _host.StopAsync();

            _host.Dispose();
        }

        /// <summary>
        /// Occurs when an exception is thrown by an application but not handled.
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            // For more info see https://docs.microsoft.com/en-us/dotnet/api/system.windows.application.dispatcherunhandledexception?view=windowsdesktop-6.0
        }
    }
}
