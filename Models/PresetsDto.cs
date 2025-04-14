namespace SnapClicker.Models;

public class PresetsDto
{
    
    public int Id { get; set; }
    public string Name { get; set; }
    public bool IsRepetitive { get; set; }
    public int RepeatCount { get; set; }
    public DateTime CreatedDate { get; set; }
    public List<RecordedAction> RecordedActions { get; set; } = new();
}