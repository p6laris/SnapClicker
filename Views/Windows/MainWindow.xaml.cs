namespace SnapClicker.Views.Windows
{
    public partial class MainWindow : INavigationWindow
    {
        public MainWindowViewModel ViewModel { get; }

        private readonly ISystemTrayService _systemTrayService;
        private bool _isExplicitShutdown;

        public MainWindow(
            MainWindowViewModel viewModel,
            INavigationViewPageProvider navigationViewPageProvider,
            INavigationService navigationService,
            ISnackbarService snackbarService,
            IContentDialogService contentDialogService,
            ISystemTrayService systemTrayService
        )
        {
            ViewModel = viewModel;
            DataContext = this;
            _systemTrayService = systemTrayService;
            
            SystemThemeWatcher.Watch(this);
            SetTheme();
            
            InitializeComponent();
            SetPageService(navigationViewPageProvider);

            navigationService.SetNavigationControl(RootNavigation);
            contentDialogService.SetDialogHost(RootContentDialog);
            snackbarService.SetSnackbarPresenter(SnackbarPresenter);

            _systemTrayService.Initialize(this);
        }

        protected override void OnClosing(CancelEventArgs e)
        {
            if (AppConfig.CloseToTray && !_isExplicitShutdown)
            {
                e.Cancel = true;
                Hide();
                return;
            }

            base.OnClosing(e);
        }

        protected override void OnStateChanged(EventArgs e)
        {
            base.OnStateChanged(e);

            if (WindowState == WindowState.Minimized && AppConfig.MinimizeToTray)
            {
                Hide();
            }
        }
        
        private void SetTheme()
        {
            var theme = AppConfig.Theme;
            ApplicationThemeManager.Apply(theme);
        }
        #region INavigationWindow methods

        public INavigationView GetNavigation() => RootNavigation;

        public bool Navigate(Type pageType) => RootNavigation.Navigate(pageType);

        public void SetPageService(INavigationViewPageProvider navigationViewPageProvider) => RootNavigation.SetPageProviderService(navigationViewPageProvider);

        public void ShowWindow() => Show();

        public void CloseWindow()
        {
            _isExplicitShutdown = true;
            Close();
        }

        #endregion INavigationWindow methods

        /// <summary>
        /// Raises the closed event.
        /// </summary>
        protected override void OnClosed(EventArgs e)
        {
            _systemTrayService.Dispose();
            base.OnClosed(e);

            // Make sure that closing this window will begin the process of closing the application.
            Application.Current.Shutdown();
        }
        
        public void SetServiceProvider(IServiceProvider serviceProvider)
        {
        }

        private void RootNavigation_OnItemInvoked(NavigationView sender, RoutedEventArgs args)
        {
            if (sender.SelectedItem?.Content is string content && content == "Github")
                Console.Write(content);
        }
    }
}
