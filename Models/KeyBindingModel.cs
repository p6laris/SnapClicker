namespace SnapClicker.Models;

public class KeyBindingModel 
{
    public Key Key { get; }
    public ModifierKeys ModifierKeys { get;}

    public KeyBindingModel(Key key, ModifierKeys modifierKeys)
    {
        Key = key;
        ModifierKeys = modifierKeys;
    }
    public IEnumerable<string> AllKeys
    {
        get
        {
            var keys = new List<string>();

            if (ModifierKeys.HasFlag(ModifierKeys.Control)) keys.Add("Ctrl");
            if (ModifierKeys.HasFlag(ModifierKeys.Shift)) keys.Add("Shift");
            if (ModifierKeys.HasFlag(ModifierKeys.Alt)) keys.Add("Alt");
            if (ModifierKeys.HasFlag(ModifierKeys.Windows)) keys.Add("Win");

            keys.Add(Key.ToString());
            return keys;
        }
    }

    public override bool Equals(object? obj)
    {
        if (obj is KeyBindingModel keyBinding)
            return Key == keyBinding.Key && ModifierKeys == keyBinding.ModifierKeys;
        
        return false;
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Key, ModifierKeys);
    }

    public override string ToString()
    {
        return $"{Key},{ModifierKeys}";
    }
}