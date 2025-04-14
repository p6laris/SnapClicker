namespace SnapClicker.Data;

public interface IPresetRepository
{
    Task<PresetsDto?> GetPresetAsync(int id);
    Task AddPresetAsync(Preset preset);

    Task<List<PresetsDto>> GetAllPresetsAsync(string? searchTerm);
    Task<List<PresetsDto>> GetAllPresetsAsync(Expression<Func<Preset, object>>? orderBy = null, bool isDescending = false);
    Task UpdatePresetAsync(Preset preset);
    Task DeletePresetAsync(int presetId);
    Task<Preset> DeleteRecordedActionAsync(int presetId, int actionId);
    Task<Preset> UpdateRecordedActionAsync(int presetId, int actionId, RecordedAction recordedAction);
}
