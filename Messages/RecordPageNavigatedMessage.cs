namespace SnapClicker.Messages;

public class RecordPageNavigatedMessage : ValueChangedMessage<bool>
{
    public RecordPageNavigatedMessage(bool value) : base(value)
    {
    }
}