namespace SnapClicker.Models;

public record RecordedAction
{
    public int Id { get; set; }
    public ActionType Type {get; set;}
    public int X { get; set; }
    public int Y { get; set; }
    public Key Key { get; set; }
    public TimeSpan Timestamp { get; set; }
    /// <summary>
    /// If manually recorded this will be true.
    /// </summary>
    public bool IsBurstMode { get; set; }
    public int PresetId { get; set; }
    public Preset Preset { get; set; }
    public string KeyString => Key.ToString();
    public string FormattedTimestamp => Timestamp.ToString(@"hh\:mm\:ss\.fff");
};