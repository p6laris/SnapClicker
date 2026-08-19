namespace SnapClicker.Messages;

public class CoordinateJitterMessage : ValueChangedMessage<(bool Enabled, int RadiusPx)>
{
    public CoordinateJitterMessage((bool Enabled, int RadiusPx) value) : base(value)
    {
    }
}
