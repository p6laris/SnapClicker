namespace SnapClicker.Helpers;

public static class DataModelsHelper
{
    public static PresetsDto ToPresetsDto(this Preset preset)
    {
        var presetsDto = new PresetsDto()
        {
            Id = preset.Id,
            Name = preset.Name,
            IsRepetitive = preset.IsRepetitive,
            RepeatCount = preset.RepeatCount,
            RecordedActions = preset.RecordedActions?.Select(a => a with { }).ToList() ?? new List<RecordedAction>(),
            CreatedDate = preset.CreatedDate,
        };
                            
        return presetsDto;
    }

    public static Preset ToPreset(this PresetsDto presetsDto)
    {
        var preset = new Preset()
        {
            Id = presetsDto.Id,
            Name = presetsDto.Name,
            IsRepetitive = presetsDto.IsRepetitive,
            RepeatCount = presetsDto.RepeatCount,
            RecordedActions = presetsDto.RecordedActions?.Select(a => a with { }).ToList() ?? new List<RecordedAction>(),
            CreatedDate = presetsDto.CreatedDate
        };
        
        return preset;
    }
}