namespace SnapClicker.Messages;

public class PreciseDelayMessage : ValueChangedMessage<bool>
{
    public PreciseDelayMessage(bool value) : base(value)
    {
    }
}