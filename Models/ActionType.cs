namespace SnapClicker.Models;

[Flags]
public enum ActionType : byte
{
    LeftMouseClick = 1 << 0,
    RightMouseClick = 1 << 1,
    MiddleMouseClick = 1 << 2,
    MouseMove = 1 << 3,
    KeyDown = 1 << 4,
    KeyUp = 1 << 5,
}
