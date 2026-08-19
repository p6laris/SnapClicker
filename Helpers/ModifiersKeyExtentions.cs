using System.Numerics;

namespace SnapClicker.Helpers;

public static class ModifiersKeyExtentions
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static int BitCount(this ModifierKeys modifiers)
    {
        return BitOperations.PopCount((uint)modifiers);
    }
}