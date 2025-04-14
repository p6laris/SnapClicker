namespace SnapClicker.Models;

public class Preset
{
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsRepetitive { get; set; } = true;
    public int RepeatCount { get; set; } = 1;
    public DateTime CreatedDate { get; set; }
    public List<RecordedAction> RecordedActions { get; set; } = new();
}