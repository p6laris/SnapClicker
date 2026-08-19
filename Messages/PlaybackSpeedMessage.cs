namespace SnapClicker.Messages
{
    public class PlaybackSpeedMessage(double value) : ValueChangedMessage<double>(value);
}
