namespace SnapClicker.Native;

public static class Constants
{ 
        public const int WhMouseLl = 14;
        public const int WhKeyboardLl = 13;
        public const int WmKeydown = 0x0100;
        public const int WmKeyup = 0x0101;
        public const int WmSyskeydown = 0x0104;
        public const int WmSyskeyup = 0x0105;
        public const int WmLbuttondown = 0x0201;
        public const int WmRbuttondown = 0x0204;
        public const int WmMbuttondown = 0x0207;
        public const int WmMousemove = 0x0200;

        // Mouse event flags
        public const uint MouseeventfLeftdown = 0x0002;
        public const uint MouseeventfLeftup = 0x0004;
        public const uint MouseeventfRightdown = 0x0008;
        public const uint MouseeventfRightup = 0x0010;
        public const uint MouseeventfMiddledown = 0x0020;
        public const uint MouseeventfMiddleup = 0x0040;

        // Keyboard event flags
        public const uint KeyeventfKeyup = 0x0002;

        // SendInput constants
        public const int InputMouse = 0;
        public const int InputKeyboard = 1;
}