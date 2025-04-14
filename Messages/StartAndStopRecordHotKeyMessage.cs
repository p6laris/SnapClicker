namespace SnapClicker.Messages;

public class StartAndStopRecordHotKeyMessage : ValueChangedMessage<KeyBindingModel>
{
    public StartAndStopRecordHotKeyMessage(KeyBindingModel value) : base(value)
    {
    }
}