namespace SnapClicker.Services
{
    public class HotKeyManager : IHotKeyManager, IDisposable
    {
        private int _id = 0;
        private IntPtr _hwnd = IntPtr.Zero;
        private HwndSource? _source;
        private readonly ConcurrentDictionary<int, (Key Key, ModifierKeys Modifiers, Action Callback)> _hotKeys = new();
        private bool _disposed = false;

        public HotKeyManager()
        {
            Application.Current.Activated += OnApplicationActivated;
            Application.Current.Exit += OnApplicationExit;
        }

        private void OnApplicationActivated(object? sender, EventArgs e)
        {
            Initialize();
        }

        private void OnApplicationExit(object? sender, EventArgs e)
        {
            Dispose();
        }

        private void Initialize()
        {
            if (Application.Current.MainWindow is null) return;

            var helper = new WindowInteropHelper(Application.Current.MainWindow);
            _hwnd = helper.Handle;

            if (_source == null)
            {
                _source = HwndSource.FromHwnd(_hwnd);
                _source?.AddHook(WndProc);
            }
        }

        public int RegisterHotKey(Key key, ModifierKeys modifiers, Action callback)
        {
            if (_disposed) throw new ObjectDisposedException(nameof(HotKeyManager));
            if (callback == null) throw new ArgumentNullException(nameof(callback));

            if (_hwnd == IntPtr.Zero)
            {
                Initialize();
                if (_hwnd == IntPtr.Zero) 
                    throw new InvalidOperationException("Window handle not available");
            }

            int id = Interlocked.Increment(ref _id);
            uint virtualKey = (uint)KeyInterop.VirtualKeyFromKey(key);

            if (!RegisterHotKey(_hwnd, id, (uint)modifiers, virtualKey))
            {
                throw new Win32Exception(Marshal.GetLastWin32Error(), 
                    $"Failed to register hotkey {key} with modifiers {modifiers}");
            }

            _hotKeys[id] = (key, modifiers, callback);
            return id;
        }

        public void UnregisterHotKey(int id)
        {
            if (_disposed) return;

            if (_hotKeys.TryRemove(id, out _) && _hwnd != IntPtr.Zero)
            {
                UnregisterHotKey(_hwnd, id);
            }
        }
        
        public bool UpdateHotKey(int id, Key newKey, ModifierKeys newModifiers) 
        {
            if (_disposed) throw new ObjectDisposedException(nameof(HotKeyManager));
    
            if (!_hotKeys.TryGetValue(id, out var oldHotkey))
            {
                return false; 
            }

            // Unregister the old hotkey
            if (_hwnd != IntPtr.Zero)
            {
                UnregisterHotKey(_hwnd, id);
            }

            uint virtualKey = (uint)KeyInterop.VirtualKeyFromKey(newKey);
            if (!RegisterHotKey(_hwnd, id, (uint)newModifiers, virtualKey))
            {
                RegisterHotKey(_hwnd, id, (uint)oldHotkey.Modifiers, 
                    (uint)KeyInterop.VirtualKeyFromKey(oldHotkey.Key));
                return false;
            }

            _hotKeys[id] = (newKey, newModifiers, oldHotkey.Callback);
            return true;
        }
        public (Key Key, ModifierKeys Modifiers)? GetHotKeyById(int id)
        {
            return _hotKeys.TryGetValue(id, out var hotkey)
                ? (hotkey.Key, hotkey.Modifiers)
                : null;
        }
        private void Cleanup()
        {
            if (_source != null)
            {
                _source.RemoveHook(WndProc);
                _source.Dispose();
                _source = null;
            }

            foreach (var id in _hotKeys.Keys)
            {
                if (_hwnd != IntPtr.Zero)
                {
                    UnregisterHotKey(_hwnd, id);
                }
            }

            _hotKeys.Clear();
        }

        private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
        {
            const int WM_HOTKEY = 0x0312;
            if (msg == WM_HOTKEY)
            {
                int id = wParam.ToInt32();
                if (_hotKeys.TryGetValue(id, out var hotkey))
                {
                    hotkey.Callback?.Invoke();
                    handled = true;
                }
            }
            return IntPtr.Zero;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    Application.Current.Activated -= OnApplicationActivated;
                    Application.Current.Exit -= OnApplicationExit;
                }

                Cleanup();
                _disposed = true;
            }
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        ~HotKeyManager()
        {
            Dispose(false);
        }

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool RegisterHotKey(IntPtr hWnd, int id, uint fsModifiers, uint vk);

        [DllImport("user32.dll", SetLastError = true)]
        private static extern bool UnregisterHotKey(IntPtr hWnd, int id);
    }
}
