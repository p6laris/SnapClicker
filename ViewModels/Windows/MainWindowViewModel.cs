using System.Windows.Documents;

namespace SnapClicker.ViewModels.Windows
{
    public partial class MainWindowViewModel : ObservableObject, IDisposable
    {
        public MainWindowViewModel()
        {
            //Github repo menu item click event.
            FooterMenuItems[0].Click += OnGithubButtonClick;
            
            //A messenger that tells when to show the record NavigationViewItem.
            WeakReferenceMessenger.Default.Register<RecordPageNavigatedMessage>(this, (_, msg) =>
            {
                var recordNavigationViewItem = MenuItems.First(m => (string)m.Tag == "Record");
                recordNavigationViewItem.Visibility = msg.Value ? Visibility.Visible : Visibility.Hidden;
            });
        }

        private void OnGithubButtonClick(object sender, RoutedEventArgs e)
        {
            Process.Start(new ProcessStartInfo
            {
                FileName = "https://github.com/p6laris/SnapClicker",
                UseShellExecute = true 
            });
        }

        [ObservableProperty]
        private string _applicationTitle = "SnapClicker";

        [ObservableProperty]
        private ObservableList<NavigationViewItem> _menuItems = new()
        {
            new NavigationViewItem()
            {
                Content = "SnapClicker",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Home24 },
                TargetPageType = typeof(DashboardPage)
            },
            new NavigationViewItem()
            {
                Content = "Record",
                Icon = new SymbolIcon { Symbol = SymbolRegular.Record24 },
                TargetPageType = typeof(RecordPage),
                Visibility = Visibility.Hidden,
                Tag = "Record"
            },
        };

        [ObservableProperty]
        private ObservableList<NavigationViewItem> _footerMenuItems = new()
        {
            new NavigationViewItem
            {
                Content = "Github",
                Icon = new SymbolIcon(SymbolRegular.Branch24),
                ToolTip = "Github Repository",
                TargetPageTag = "https://github.com/p6laris/SnapClicker"
            },
            new NavigationViewItem()
            {
                Content = "Settings",
                Icon = new SymbolIcon (SymbolRegular.Settings24),
                TargetPageType = typeof(SettingsPage)
            }
        };
        public void Dispose()
        {
            WeakReferenceMessenger.Default.Unregister<RecordPageNavigatedMessage>(this);
        }
    }
}
