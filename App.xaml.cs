namespace SnapClicker
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        static App()
        {
            var logDir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "SnapClicker",
                "Logs"
            );

            if (!Directory.Exists(logDir))
                Directory.CreateDirectory(logDir);

            var logFilePath = Path.Combine(logDir, "snapclicker-.log");

            Log.Logger = new LoggerConfiguration()
                .MinimumLevel.Information()
                .WriteTo.File(
                    logFilePath,
                    rollingInterval: RollingInterval.Day,
                    retainedFileCountLimit: 7,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}"
                )
                .CreateLogger();

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                if (e.ExceptionObject is Exception ex)
                    Log.Fatal(ex, "Unhandled AppDomain Fatal Exception");
                else
                    Log.Fatal("Unhandled AppDomain Fatal Exception: {ExceptionObject}", e.ExceptionObject);
                Log.CloseAndFlush();
            };

            TaskScheduler.UnobservedTaskException += (s, e) =>
            {
                Log.Error(e.Exception, "Unobserved Task Exception");
                e.SetObserved();
            };
        }

        private static readonly IHost _host = Host
            .CreateDefaultBuilder()
            .UseSerilog()
            .ConfigureAppConfiguration(c => { c.SetBasePath(Path.GetDirectoryName(AppContext.BaseDirectory) ?? AppContext.BaseDirectory); })
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
                services.AddSingleton<CountdownWindow>();
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
                services.AddSingleton<CountdownWindowViewModel>();
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
            Log.Information("SnapClicker application starting up.");

            try
            {
                VelopackApp.Build().Run();
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Velopack initialization warning");
            }
            
            try
            {
                await SetDatabase();
                await _host.StartAsync();
            }
            catch (Exception ex)
            {
                Log.Fatal(ex, "Failed during application startup initialization.");
                throw;
            }
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
            Log.Information("SnapClicker application shutting down.");

            await _host.StopAsync();
            _host.Dispose();
            Log.CloseAndFlush();
        }

        /// <summary>
        /// Occurs when an exception is thrown by an application but not handled.
        /// </summary>
        private void OnDispatcherUnhandledException(object sender, DispatcherUnhandledExceptionEventArgs e)
        {
            Log.Error(e.Exception, "Unhandled Dispatcher Exception on UI thread");

            System.Windows.MessageBox.Show(
                $"An unexpected error occurred:\n{e.Exception.Message}\n\nCheck logs in %AppData%\\SnapClicker\\Logs for details.",
                "SnapClicker Error",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error
            );

            e.Handled = true;
        }
    }
}
