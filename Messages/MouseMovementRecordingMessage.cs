namespace SnapClicker.Messages;

public class MouseMovementRecordingMessage : ValueChangedMessage<bool>
{
    public MouseMovementRecordingMessage(bool value) : base(value)
    {
    }
}