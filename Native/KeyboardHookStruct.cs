namespace SnapClicker.Native;

[StructLayout(LayoutKind.Sequential)]
public struct KeyboardHookStruct 
{
    public uint vkCode;
    public uint scanCode;
    public uint flags;
    public uint time;
    public IntPtr dwExtraInfo;
}
