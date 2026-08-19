namespace SnapClicker.Models;

[Flags]
public enum ActionType : ushort
{
    LeftMouseClick = 1 << 0,
    RightMouseClick = 1 << 1,
    MiddleMouseClick = 1 << 2,
    MouseMove = 1 << 3,
    KeyDown = 1 << 4,
    KeyUp = 1 << 5,
    LeftMouseDown = 1 << 6,
    LeftMouseUp = 1 << 7,
    RightMouseDown = 1 << 8,
    RightMouseUp = 1 << 9,
    MiddleMouseDown = 1 << 10,
    MiddleMouseUp = 1 << 11,
}
