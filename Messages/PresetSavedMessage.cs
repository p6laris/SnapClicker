namespace SnapClicker.Messages;
public class PresetSavedMessage(bool value) : ValueChangedMessage<bool>(value);