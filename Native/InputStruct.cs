namespace SnapClicker.Native;

[StructLayout(LayoutKind.Sequential)]
public struct InputStruct 
{
    public uint type;
    public InputUnion u;
}
