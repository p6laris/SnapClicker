namespace SnapClicker.Services;
public interface IHotKeyManager
{
    int RegisterHotKey(Key key, ModifierKeys modifiers, Action callback);
    (Key Key, ModifierKeys Modifiers)? GetHotKeyById(int id);
    void UnregisterHotKey(int id);
    bool UpdateHotKey(int id, Key newKey, ModifierKeys newModifiers);
}
