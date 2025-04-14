namespace SnapClicker.Native
{
    public sealed class KeyboardHook : IDisposable
    {
        private IntPtr _keyboardHookId = IntPtr.Zero;
        private readonly Callbacks.LowLevelKeyboardProc _keyboardProc;
        private readonly Stopwatch _stopwatch = new Stopwatch();

        public event Action<Key, TimeSpan>? OnKeyDown;
        public event Action<Key, TimeSpan>? OnKeyUp;
        public KeyboardHook()
        {
            _keyboardProc = KeyboardHookCallback;
        }

        public void Start()
        {
            _stopwatch.Restart();
            _keyboardHookId = Methods.SetWindowsHookEx(Constants.WhKeyboardLl, _keyboardProc, Methods.GetModuleHandle(null), 0);
        }

        public void Stop()
        {
            _stopwatch.Stop();
            if (_keyboardHookId != IntPtr.Zero)
            {
                Methods.UnhookWindowsHookEx(_keyboardHookId);
                _keyboardHookId = IntPtr.Zero;
            }
        }


        private IntPtr KeyboardHookCallback(int nCode, IntPtr wParam, IntPtr lParam)
        {
            if (nCode >= 0)
            {
                var hookStruct = Marshal.PtrToStructure<KeyboardHookStruct>(lParam);
                Key keyPressed = KeyInterop.KeyFromVirtualKey((int)hookStruct.vkCode);

                TimeSpan timestamp = TimeSpan.FromMilliseconds(_stopwatch.ElapsedMilliseconds);
                bool isKeyDown = wParam == Constants.WmKeydown || wParam == Constants.WmSyskeydown;
                bool isKeyUp = wParam == Constants.WmKeyup || wParam == Constants.WmSyskeyup;

                if (isKeyDown)
                {
                    OnKeyDown?.Invoke(keyPressed, timestamp);
                }
                else if (isKeyUp)
                {
                    OnKeyUp?.Invoke(keyPressed, timestamp);
                }
            }

            return Methods.CallNextHookEx(_keyboardHookId, nCode, wParam, lParam);
        }

        public static void SimulateKeyDown(Key key)
        {
            int vk = KeyInterop.VirtualKeyFromKey(key);
            var input = CreateKeyboardInput((ushort)vk, 0);
            Methods.SendInput(1, new[] { input }, Marshal.SizeOf<InputStruct>());
        }

        public static void SimulateKeyUp(Key key)
        {
            int vk = KeyInterop.VirtualKeyFromKey(key);
            var input = CreateKeyboardInput((ushort)vk, Constants.KeyeventfKeyup);
            Methods.SendInput(1, new[] { input }, Marshal.SizeOf<InputStruct>());
        }

        private static InputStruct CreateKeyboardInput(ushort vk, uint flags)
        {
            return new InputStruct()
            {
                type = Constants.InputKeyboard,
                u = new InputUnion
                {
                    ki = new KeyboardInput() 
                    {
                        wVk = vk,
                        dwFlags = flags
                    }
                }
            };
        }

        public void Dispose()
        {
            Stop();
            GC.SuppressFinalize(this);
        }

        ~KeyboardHook()
        {
            Dispose();
        }
    }
}
