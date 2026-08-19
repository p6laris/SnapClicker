namespace SnapClicker.Helpers;

public static class DataModelsHelper
{
    public static PresetsDto ToPresetsDto(this Preset preset)
    {
        var actions = preset.RecordedActions;
        List<RecordedAction> clonedActions;
        if (actions != null && actions.Count > 0)
        {
            clonedActions = new List<RecordedAction>(actions.Count);
            for (int i = 0; i < actions.Count; i++)
            {
                clonedActions.Add(actions[i] with { });
            }
        }
        else
        {
            clonedActions = new List<RecordedAction>(0);
        }

        return new PresetsDto
        {
            Id = preset.Id,
            Name = preset.Name,
            IsRepetitive = preset.IsRepetitive,
            RepeatCount = preset.RepeatCount,
            RecordedActions = clonedActions,
            CreatedDate = preset.CreatedDate,
        };
    }

    public static Preset ToPreset(this PresetsDto presetsDto)
    {
        var actions = presetsDto.RecordedActions;
        List<RecordedAction> clonedActions;
        if (actions != null && actions.Count > 0)
        {
            clonedActions = new List<RecordedAction>(actions.Count);
            for (int i = 0; i < actions.Count; i++)
            {
                clonedActions.Add(actions[i] with { });
            }
        }
        else
        {
            clonedActions = new List<RecordedAction>(0);
        }

        return new Preset
        {
            Id = presetsDto.Id,
            Name = presetsDto.Name,
            IsRepetitive = presetsDto.IsRepetitive,
            RepeatCount = presetsDto.RepeatCount,
            RecordedActions = clonedActions,
            CreatedDate = presetsDto.CreatedDate
        };
    }
}