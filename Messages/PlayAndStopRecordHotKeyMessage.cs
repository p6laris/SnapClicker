namespace SnapClicker.Messages;

public class PlayAndStopRecordHotKeyMessage : ValueChangedMessage<KeyBindingModel>
{
    public PlayAndStopRecordHotKeyMessage(KeyBindingModel value) : base(value)
    {
    }
}