namespace SnapClicker.Messages;

public class ActionIntervalMessage : ValueChangedMessage<double>
{
    public ActionIntervalMessage(double value) : base(value)
    {
    }
}