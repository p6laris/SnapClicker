namespace SnapClicker.Native
{
    public sealed class MouseHook : IDisposable
    {
        private IntPtr _mouseHookId = IntPtr.Zero;
        private readonly Callbacks.LowLevelMouseProc _mouseProc;
        private readonly Stopwatch _stopwatch = new Stopwatch();
        private bool _isDisposed;
        
        public event Action<int, int, ActionType, TimeSpan>? OnMouseAction;

        public MouseHook()
        {
            _mouseProc = MouseHookCallback;
        }

        public void Start()
        {
            if (_mouseHookId != IntPtr.Zero)
                return; // Already running

            _stopwatch.Restart();
            _mouseHookId = Methods.SetWindowsHookEx(Constants.WhMouseLl, _mouseProc, Methods.GetModuleHandle(null), 0);

            if (_mouseHookId == IntPtr.Zero)
                throw new InvalidOperationException("Failed to set mouse hook.");
        }

        public void Stop()
        {
            if (_mouseHookId != IntPtr.Zero)
            {
                Methods.UnhookWindowsHookEx(_mouseHookId);
                _mouseHookId = IntPtr.Zero;
                _stopwatch.Stop();
            }
        }

        private IntPtr MouseHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode < 0)
                return Methods.CallNextHookEx(_mouseHookId, nCode, wParam, lParam);

            var hookStruct = Marshal.PtrToStructure<MouseHookStruct>(lParam);
            TimeSpan timestamp = TimeSpan.FromMilliseconds(_stopwatch.ElapsedMilliseconds);

            switch ((int)wParam)
            {
                case Constants.WmLbuttondown:
                    OnMouseAction?.Invoke(hookStruct.pt.X, hookStruct.pt.Y, ActionType.LeftMouseClick, timestamp);
                    break;
                case Constants.WmRbuttondown:
                    OnMouseAction?.Invoke(hookStruct.pt.X, hookStruct.pt.Y, ActionType.RightMouseClick, timestamp);
                    break;
                case Constants.WmMbuttondown:
                    OnMouseAction?.Invoke(hookStruct.pt.X, hookStruct.pt.Y, ActionType.MiddleMouseClick, timestamp);
                    break;
                case Constants.WmMousemove:
                    OnMouseAction?.Invoke(hookStruct.pt.X, hookStruct.pt.Y, ActionType.MouseMove, timestamp);
                    break;
            }

            return Methods.CallNextHookEx(_mouseHookId, nCode, wParam, lParam);
        }

        public static void SimulateLeftClick(int x, int y)
        {
            Methods.SetCursorPos(x, y);

            Span<InputStruct> inputs = stackalloc InputStruct[2];
            inputs[0] = CreateMouseInput(Constants.MouseeventfLeftdown);
            inputs[1] = CreateMouseInput(Constants.MouseeventfLeftup);

            Methods.SendInput(2, inputs.ToArray(), Marshal.SizeOf<InputStruct>());
        }

        public static void SimulateRightClick(int x, int y)
        {
            Methods.SetCursorPos(x, y);

            Span<InputStruct> inputs = stackalloc InputStruct[2];
            inputs[0] = CreateMouseInput(Constants.MouseeventfRightdown);
            inputs[1] = CreateMouseInput(Constants.MouseeventfRightup);

            Methods.SendInput(2, inputs.ToArray(), Marshal.SizeOf<InputStruct>());
        }

        public static void SimulateMiddleClick(int x, int y)
        {
            Methods.SetCursorPos(x, y);

            Span<InputStruct> inputs = stackalloc InputStruct[2];
            inputs[0] = CreateMouseInput(Constants.MouseeventfMiddledown);
            inputs[1] = CreateMouseInput(Constants.MouseeventfMiddleup);

            Methods.SendInput(2, inputs.ToArray(), Marshal.SizeOf<InputStruct>());
        }

        private static  InputStruct CreateMouseInput(uint flags)
        {
            return new InputStruct()
            {
                type = Constants.InputMouse,
                u = new InputUnion
                {
                    mi = new MouseInput()
                    {
                        dwFlags = flags
                    }
                }
            };
        }

        public void Dispose()
        {
            if (!_isDisposed)
            {
                Stop();
                _isDisposed = true;
            }
            GC.SuppressFinalize(this);
        }

        ~MouseHook()
        {
            Dispose();
        }
    }
}
