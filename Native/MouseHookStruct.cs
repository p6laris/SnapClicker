namespace SnapClicker.Native;

[StructLayout(LayoutKind.Sequential)]
public struct MouseHookStruct
{
    public PointStruct pt;
    public uint mouseData;
    public uint flags;
    public uint time;
    public IntPtr dwExtraInfo;
}
