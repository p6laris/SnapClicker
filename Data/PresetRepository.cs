namespace SnapClicker.Data;

public class PresetRepository(SnapClickerDbContext context) : IPresetRepository
{
    public async Task<PresetsDto?> GetPresetAsync(int id)
    {
        var preset = await context.Presets.FindAsync(id);
        return preset?.ToPresetsDto();
    }

    public async Task AddPresetAsync(Preset preset)
    {
        context.Presets.Add(preset);
        await context.SaveChangesAsync();
    }

    public async Task<List<PresetsDto>> GetAllPresetsAsync(string? searchTerm)
    {
        var query = context.Presets
            .AsNoTracking()
            .Include(p => p.RecordedActions)
            .AsQueryable();

        if (!string.IsNullOrEmpty(searchTerm))
        {
            query = query.Where(p => EF.Functions.Like(p.Name, $"%{searchTerm}%"));
        }

        return await query
            .Select(p => p.ToPresetsDto())
            .ToListAsync();
    }

    public async Task<List<PresetsDto>> GetAllPresetsAsync(
        Expression<Func<Preset, object>>? orderBy = null, 
        bool isDescending = false)
    {
        var query = context.Presets
            .AsNoTracking()
            .Include(p => p.RecordedActions)
            .AsQueryable();
    
        if (orderBy != null)
        {
            query = isDescending 
                ? query.OrderByDescending(orderBy) 
                : query.OrderBy(orderBy);
        }

        return await query
            .Select(p => p.ToPresetsDto())
            .ToListAsync();
    }

    public async Task UpdatePresetAsync(Preset preset)
    {
        var pre = await context.Presets.FindAsync(preset.Id);
        if (pre == null)
            throw new InvalidOperationException($"Preset with ID {preset.Id} not found.");

        context.Entry(pre).CurrentValues.SetValues(preset);
        await context.SaveChangesAsync();
    }

    public async Task DeletePresetAsync(int presetId)
    {
        var pre = await context.Presets.FindAsync(presetId);
        if (pre == null)
            throw new InvalidOperationException($"Preset with ID {presetId} not found.");

        context.Presets.Remove(pre);
        await context.SaveChangesAsync();
    }

    public async Task<Preset> DeleteRecordedActionAsync(int presetId, int actionId)
    {
        var preset = await context.Presets
            .Include(p => p.RecordedActions)
            .FirstOrDefaultAsync(p => p.Id == presetId);

        if (preset == null)
            throw new InvalidOperationException($"Preset with ID {presetId} not found.");

        var actionToDelete = preset.RecordedActions.FirstOrDefault(a => a.Id == actionId);
        if (actionToDelete == null)
            throw new InvalidOperationException($"RecordedAction with ID {actionId} not found in Preset {presetId}.");

        // Remove the recorded action explicitly from the database
        context.RecordedActions.Remove(actionToDelete);
        await context.SaveChangesAsync();

        // If no recorded actions remain, remove the preset
        if (!preset.RecordedActions.Any())
        {
            context.Presets.Remove(preset);
            await context.SaveChangesAsync();
        }

        return preset;
    }

    public async Task<Preset> UpdateRecordedActionAsync(int presetId, int actionId, RecordedAction recordedAction)
    {
        var preset = await context.Presets
            .Include(p => p.RecordedActions)
            .FirstOrDefaultAsync(p => p.Id == presetId);

        if (preset == null)
            throw new InvalidOperationException($"Preset with ID {presetId} not found.");

        var actionToUpdate = preset.RecordedActions.FirstOrDefault(a => a.Id == actionId);
        if (actionToUpdate == null)
            throw new InvalidOperationException($"RecordedAction with ID {actionId} not found in Preset {presetId}.");

        // Update the action values
        context.Entry(actionToUpdate).CurrentValues.SetValues(recordedAction);
        await context.SaveChangesAsync();

        // Reload the preset to get the latest state including the updated action
        await context.Entry(preset).ReloadAsync();
        return preset;
    }
}
