namespace SnapClicker.Helpers;

public static class ModifiersKeyExtentions
{
    public static int BitCount(this ModifierKeys modifiers)
    {
        uint n = (uint)modifiers;
        int count = 0;
        while (n != 0)
        {
            n &= (n - 1); // Clears the lowest set bit
            count++;
        }
        return count;
    }
}