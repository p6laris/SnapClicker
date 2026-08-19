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
                    OnMouseAction?.Invoke(hookStruct.pt.X, hookStruct.pt.Y, ActionType.LeftMouseDown, timestamp);
                    break;
                case Constants.WmLbuttonup:
                    OnMouseAction?.Invoke(hookStruct.pt.X, hookStruct.pt.Y, ActionType.LeftMouseUp, timestamp);
                    break;
                case Constants.WmRbuttondown:
                    OnMouseAction?.Invoke(hookStruct.pt.X, hookStruct.pt.Y, ActionType.RightMouseDown, timestamp);
                    break;
                case Constants.WmRbuttonup:
                    OnMouseAction?.Invoke(hookStruct.pt.X, hookStruct.pt.Y, ActionType.RightMouseUp, timestamp);
                    break;
                case Constants.WmMbuttondown:
                    OnMouseAction?.Invoke(hookStruct.pt.X, hookStruct.pt.Y, ActionType.MiddleMouseDown, timestamp);
                    break;
                case Constants.WmMbuttonup:
                    OnMouseAction?.Invoke(hookStruct.pt.X, hookStruct.pt.Y, ActionType.MiddleMouseUp, timestamp);
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

            var inputs = new[]
            {
                CreateMouseInput(Constants.MouseeventfLeftdown),
                CreateMouseInput(Constants.MouseeventfLeftup)
            };

            Methods.SendInput(2, inputs, Marshal.SizeOf<InputStruct>());
        }

        public static void SimulateLeftDown(int x, int y)
        {
            Methods.SetCursorPos(x, y);
            var input = CreateMouseInput(Constants.MouseeventfLeftdown);
            Methods.SendInput(1, ref input, Marshal.SizeOf<InputStruct>());
        }

        public static void SimulateLeftUp(int x, int y)
        {
            Methods.SetCursorPos(x, y);
            var input = CreateMouseInput(Constants.MouseeventfLeftup);
            Methods.SendInput(1, ref input, Marshal.SizeOf<InputStruct>());
        }

        public static void SimulateRightClick(int x, int y)
        {
            Methods.SetCursorPos(x, y);

            var inputs = new[]
            {
                CreateMouseInput(Constants.MouseeventfRightdown),
                CreateMouseInput(Constants.MouseeventfRightup)
            };

            Methods.SendInput(2, inputs, Marshal.SizeOf<InputStruct>());
        }

        public static void SimulateRightDown(int x, int y)
        {
            Methods.SetCursorPos(x, y);
            var input = CreateMouseInput(Constants.MouseeventfRightdown);
            Methods.SendInput(1, ref input, Marshal.SizeOf<InputStruct>());
        }

        public static void SimulateRightUp(int x, int y)
        {
            Methods.SetCursorPos(x, y);
            var input = CreateMouseInput(Constants.MouseeventfRightup);
            Methods.SendInput(1, ref input, Marshal.SizeOf<InputStruct>());
        }

        public static void SimulateMiddleClick(int x, int y)
        {
            Methods.SetCursorPos(x, y);

            var inputs = new[]
            {
                CreateMouseInput(Constants.MouseeventfMiddledown),
                CreateMouseInput(Constants.MouseeventfMiddleup)
            };

            Methods.SendInput(2, inputs, Marshal.SizeOf<InputStruct>());
        }

        public static void SimulateMiddleDown(int x, int y)
        {
            Methods.SetCursorPos(x, y);
            var input = CreateMouseInput(Constants.MouseeventfMiddledown);
            Methods.SendInput(1, ref input, Marshal.SizeOf<InputStruct>());
        }

        public static void SimulateMiddleUp(int x, int y)
        {
            Methods.SetCursorPos(x, y);
            var input = CreateMouseInput(Constants.MouseeventfMiddleup);
            Methods.SendInput(1, ref input, Marshal.SizeOf<InputStruct>());
        }

        public static void SimulateMouseMove(int x, int y)
        {
            Methods.SetCursorPos(x, y);
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
