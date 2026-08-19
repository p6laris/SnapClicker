namespace SnapClicker.Services;

public class SystemTrayService : ISystemTrayService
{
    private Window? _mainWindow;
    private IntPtr _hwnd;
    private IntPtr _hIcon;
    private NOTIFYICONDATA _notifyIconData;
    private bool _isInitialized;
    private HwndSource? _hwndSource;

    public void Initialize(Window mainWindow)
    {
        if (_isInitialized) return;
        _mainWindow = mainWindow;

        var helper = new WindowInteropHelper(mainWindow);
        _hwnd = helper.Handle;
        if (_hwnd == IntPtr.Zero)
        {
            mainWindow.SourceInitialized += (s, e) =>
            {
                _hwnd = new WindowInteropHelper(mainWindow).Handle;
                SetupTray();
            };
        }
        else
        {
            SetupTray();
        }
    }

    private void SetupTray()
    {
        if (_isInitialized || _hwnd == IntPtr.Zero) return;

        try
        {
            var iconStream = Application.GetResourceStream(new Uri("pack://application:,,,/SnapClicker1024.ico"))?.Stream;
            if (iconStream != null)
            {
                var icon = new System.Drawing.Icon(iconStream);
                _hIcon = icon.Handle;
            }
        }
        catch
        {
            // fallback
        }

        _notifyIconData = new NOTIFYICONDATA
        {
            cbSize = (uint)Marshal.SizeOf<NOTIFYICONDATA>(),
            hWnd = _hwnd,
            uID = 1001,
            uFlags = TrayNativeConstants.NIF_MESSAGE | TrayNativeConstants.NIF_ICON | TrayNativeConstants.NIF_TIP,
            uCallbackMessage = TrayNativeConstants.WM_TRAYICON,
            hIcon = _hIcon,
            szTip = "SnapClicker - Auto Clicker & Macro Recorder"
        };

        Methods.Shell_NotifyIcon(TrayNativeConstants.NIM_ADD, ref _notifyIconData);

        _hwndSource = HwndSource.FromHwnd(_hwnd);
        _hwndSource?.AddHook(WndProc);

        _isInitialized = true;
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == TrayNativeConstants.WM_TRAYICON)
        {
            int mouseMsg = lParam.ToInt32() & 0xFFFF;
            if (mouseMsg == TrayNativeConstants.WM_LBUTTONUP || mouseMsg == TrayNativeConstants.WM_LBUTTONDBLCLK)
            {
                ToggleMainWindow();
                handled = true;
            }
            else if (mouseMsg == TrayNativeConstants.WM_RBUTTONUP)
            {
                ShowContextMenu();
                handled = true;
            }
        }
        return IntPtr.Zero;
    }

    public void ToggleMainWindow()
    {
        if (_mainWindow == null) return;

        if (_mainWindow.IsVisible && _mainWindow.WindowState != WindowState.Minimized)
        {
            _mainWindow.WindowState = WindowState.Minimized;
            if (AppConfig.MinimizeToTray)
                _mainWindow.Hide();
        }
        else
        {
            RestoreMainWindow();
        }
    }

    public void RestoreMainWindow()
    {
        if (_mainWindow == null) return;

        _mainWindow.Show();
        _mainWindow.WindowState = WindowState.Normal;
        _mainWindow.Activate();
        Methods.SetForegroundWindow(_hwnd);
    }

    private void ShowContextMenu()
    {
        if (_mainWindow == null) return;

        var contextMenu = new ContextMenu();

        var openItem = new Wpf.Ui.Controls.MenuItem
        {
            Header = "Open SnapClicker",
            Icon = new SymbolIcon { Symbol = SymbolRegular.Window24 },
            FontWeight = FontWeights.SemiBold
        };
        openItem.Click += (s, e) => RestoreMainWindow();
        contextMenu.Items.Add(openItem);

        contextMenu.Items.Add(new Separator());

        var settingsItem = new Wpf.Ui.Controls.MenuItem
        {
            Header = "Settings",
            Icon = new SymbolIcon { Symbol = SymbolRegular.Settings24 }
        };
        settingsItem.Click += (s, e) =>
        {
            RestoreMainWindow();
            if (_mainWindow is INavigationWindow navWindow)
            {
                navWindow.Navigate(typeof(SettingsPage));
            }
        };
        contextMenu.Items.Add(settingsItem);

        contextMenu.Items.Add(new Separator());

        var exitItem = new Wpf.Ui.Controls.MenuItem
        {
            Header = "Exit",
            Icon = new SymbolIcon { Symbol = SymbolRegular.Power24 }
        };
        exitItem.Click += (s, e) =>
        {
            RemoveTrayIcon();
            Application.Current.Shutdown();
        };
        contextMenu.Items.Add(exitItem);

        Methods.GetCursorPos(out var pt);
        contextMenu.Placement = System.Windows.Controls.Primitives.PlacementMode.AbsolutePoint;
        contextMenu.HorizontalOffset = pt.X;
        contextMenu.VerticalOffset = pt.Y;
        contextMenu.IsOpen = true;

        Methods.SetForegroundWindow(_hwnd);
    }

    public void ShowNotification(string title, string message)
    {
        if (!_isInitialized) return;

        var data = _notifyIconData;
        data.uFlags |= TrayNativeConstants.NIF_INFO;
        data.szInfoTitle = title;
        data.szInfo = message;
        data.dwInfoFlags = TrayNativeConstants.NIIF_INFO;

        Methods.Shell_NotifyIcon(TrayNativeConstants.NIM_MODIFY, ref data);
    }

    public void RemoveTrayIcon()
    {
        if (_isInitialized)
        {
            Methods.Shell_NotifyIcon(TrayNativeConstants.NIM_DELETE, ref _notifyIconData);
            _isInitialized = false;
        }

        if (_hIcon != IntPtr.Zero)
        {
            Methods.DestroyIcon(_hIcon);
            _hIcon = IntPtr.Zero;
        }
    }

    public void Dispose()
    {
        RemoveTrayIcon();
        if (_hwndSource != null)
        {
            _hwndSource.RemoveHook(WndProc);
            _hwndSource = null;
        }
    }
}
