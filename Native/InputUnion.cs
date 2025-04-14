namespace SnapClicker.Native;

[StructLayout(LayoutKind.Explicit)]
public struct InputUnion
{
    [FieldOffset(0)]
    public MouseInput mi;
    [FieldOffset(0)]
    public KeyboardInput ki;
}
