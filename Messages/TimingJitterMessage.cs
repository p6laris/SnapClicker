namespace SnapClicker.Messages;

public class TimingJitterMessage : ValueChangedMessage<(bool Enabled, int RangeMs)>
{
    public TimingJitterMessage((bool Enabled, int RangeMs) value) : base(value)
    {
    }
}
