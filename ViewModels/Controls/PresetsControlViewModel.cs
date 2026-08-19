namespace SnapClicker.ViewModels.Controls;

public partial class PresetsControlViewModel : ObservableObject, IDisposable
{
    private readonly IPresetRepository _presetRepository;
    private readonly IContentDialogService _dialogService;
    private readonly INavigationService _navigationService;
    private readonly ISnackbarService _snackbarService;
    private readonly RecordPageViewModel _recordPageViewModel;
    
    private ObservableList<PresetsDto> _presetsList = new();
    
    public IWritableSynchronizedViewList<PresetsDto> PresetsView { get; set; }
    
    [ObservableProperty] private bool _hasPresets;
    [ObservableProperty] private PresetSortCriteria _presetSortCriteria;
    [ObservableProperty] private string _searchTerm = string.Empty;
    
    public PresetsControlViewModel(IPresetRepository presetRepository,
        RecordPageViewModel recordPageViewModel,
        INavigationService navigationService, 
        ISnackbarService snackbarService,
        IContentDialogService contentDialogService)
    {
        _presetRepository = presetRepository;
        _recordPageViewModel = recordPageViewModel;
        _dialogService = contentDialogService;
        _snackbarService = snackbarService;
        _navigationService = navigationService;

        PresetsView = _presetsList.ToWritableNotifyCollectionChanged();
        
        //When we created a new preset we reload the data from the data source.
        WeakReferenceMessenger.Default.Register<PresetSavedMessage>(this, (_, _) => { _ = ReloadPresets(); });
    }
    
    [RelayCommand]
    public async Task LoadPresets()
    {
        try
        {
            var presets = await _presetRepository.GetAllPresetsAsync();

            _presetsList.Clear();
            _presetsList.AddRange(presets);
            HasPresets = _presetsList.Count > 0;
        }
        catch
        {
            ShowErrorMessage("Can't Load Presets", "Failed to load saved presets. Try restarting the app.", new SymbolIcon(SymbolRegular.DatabaseWarning20));
            HasPresets = false;
        }
    }

    [RelayCommand]
    public async Task FilterPresetsAsync(PresetSortCriteria criteria)
    {
        List<PresetsDto>? presets = criteria switch
        {
            PresetSortCriteria.Ascending => await _presetRepository.GetAllPresetsAsync(p => p.Name),
            PresetSortCriteria.Descending => await _presetRepository.GetAllPresetsAsync(p => p.Name, isDescending: true),
            PresetSortCriteria.Date => await _presetRepository.GetAllPresetsAsync(p => p.CreatedDate),
            PresetSortCriteria.ActionCount => await _presetRepository.GetAllPresetsAsync(p => p.RecordedActions.Count, isDescending: true),
            _ => await _presetRepository.GetAllPresetsAsync()
        };
        
        _presetsList.Clear();
        _presetsList.AddRange(presets);
    }
    [RelayCommand]
    public async Task SearchPresetsAsync()
    {
        try
        {
            var presets = await _presetRepository.GetAllPresetsAsync(searchTerm: SearchTerm);
            _presetsList.Clear();
            _presetsList.AddRange(presets);
        }
        catch
        {
            ShowErrorMessage("Search presets", $"Couldn't search presets!", new SymbolIcon(SymbolRegular.BookDatabase20));
        }
    }

    [RelayCommand]
    public async Task EditPresetAsync(PresetsDto presetsDto)
    {
        var preset = presetsDto.ToPreset();
        var result = await new EditPresetDialog(_dialogService.GetDialogHostEx(), preset).ShowAsync();
        
        if (result != ContentDialogResult.Primary) 
            return;
        
        if (string.IsNullOrEmpty(preset.Name))
        {
            ShowErrorMessage("Name Required", "Preset name can't be empty. Try again and enter a name to continue.", new SymbolIcon(SymbolRegular.TextGrammarWand20));
            return;
        }

        try
        {
            await _presetRepository.UpdatePresetAsync(preset);
            WeakReferenceMessenger.Default.Send(new PresetSavedMessage(true));
        }
        catch
        {
            ShowErrorMessage("Save Failed", $"Couldn't save '{preset.Name}'.", new SymbolIcon(SymbolRegular.BookDatabase20));
        }
    }

    [RelayCommand]
    public async Task DeletePresetAsync(PresetsDto preset)
    {
        try
        {
            await _presetRepository.DeletePresetAsync(preset.Id);
            _presetsList.Remove(preset);
            HasPresets = _presetsList.Any();
        }
        catch 
        {
            ShowErrorMessage("Delete Failed", $"Couldn't delete '{preset.Name}'. Try again or restart the app.", new SymbolIcon(SymbolRegular.DeleteOff20));
        }
    }
    [RelayCommand]
    public async Task EditAction(RecordedAction action)
    {
        var result = await new PresetActionEdit(_dialogService.GetDialogHostEx(), action).ShowAsync();
        if (result != ContentDialogResult.Primary) return;

        try
        {
            var viewModel = App.Services.GetRequiredService<PresetActionEditViewModel>();
            var parentPreset = _presetsList.FirstOrDefault(p => p.RecordedActions.Any(a => a.Id == action.Id))
                               ?? throw new InvalidOperationException("Parent preset not found for the given action.");

            action.Key = viewModel.Key;
            action.X = viewModel.CursorX;
            action.Y = viewModel.CursorY;
            action.Type = viewModel.ActionType;

            var preset = await _presetRepository.UpdateRecordedActionAsync(parentPreset.Id, action.Id, action);

            int index = _presetsList.IndexOf(parentPreset);

            _presetsList[index] = preset.ToPresetsDto();
        }
        catch 
        {
            ShowErrorMessage("Edit Failed", $"Couldn't update action.", new SymbolIcon(SymbolRegular.EditOff20));
        }
    }

    [RelayCommand]
    public void LoadPreset(PresetsDto preset)
    {
        _recordPageViewModel.SelectedPreset = preset;
        WeakReferenceMessenger.Default.Send(new RecordPageNavigatedMessage(true));
        _navigationService.Navigate(typeof(RecordPage));
    }
    [RelayCommand]
    public async Task DeleteAction(int actionId)
    {
        try
        {
            var parentPreset = _presetsList.FirstOrDefault(p => p.RecordedActions.Any(a => a.Id == actionId))
                               ?? throw new InvalidOperationException("Parent preset not found for the given action.");

            var preset = await _presetRepository.DeleteRecordedActionAsync(parentPreset.Id, actionId);
            if (!preset.RecordedActions.Any())
                _presetsList.Remove(parentPreset);

            else
            {
                int index = _presetsList.IndexOf(parentPreset);

                _presetsList[index] = preset.ToPresetsDto();
            }
        }
        catch
        {
            ShowErrorMessage("Delete Failed", "Couldn't remove action.", new SymbolIcon(SymbolRegular.DeleteOff20));
        }
    }

    [RelayCommand]
    public async Task ExportPresetAsync(PresetsDto presetDto)
    {
        try
        {
            var sfd = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Export Preset",
                FileName = $"{presetDto.Name}.json",
                DefaultExt = ".json",
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*"
            };

            if (sfd.ShowDialog() == true)
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Converters = { new JsonStringEnumConverter() }
                };

                var exportData = new Preset
                {
                    Name = presetDto.Name,
                    IsRepetitive = presetDto.IsRepetitive,
                    RepeatCount = presetDto.RepeatCount,
                    CreatedDate = presetDto.CreatedDate,
                    RecordedActions = presetDto.RecordedActions.Select(a => new RecordedAction
                    {
                        Type = a.Type,
                        X = a.X,
                        Y = a.Y,
                        Key = a.Key,
                        Timestamp = a.Timestamp,
                        IsBurstMode = a.IsBurstMode
                    }).ToList()
                };

                var json = JsonSerializer.Serialize(exportData, options);
                await File.WriteAllTextAsync(sfd.FileName, json);

                _snackbarService.Show(
                    "Preset Exported",
                    $"Successfully exported '{presetDto.Name}'.",
                    ControlAppearance.Success,
                    new SymbolIcon(SymbolRegular.CheckmarkCircle20),
                    TimeSpan.FromSeconds(3)
                );
            }
        }
        catch (Exception ex)
        {
            ShowErrorMessage("Export Failed", $"Could not export preset: {ex.Message}", new SymbolIcon(SymbolRegular.ErrorCircle20));
        }
    }

    [RelayCommand]
    public async Task ExportAllPresetsAsync()
    {
        try
        {
            if (!_presetsList.Any())
                return;

            var sfd = new Microsoft.Win32.SaveFileDialog
            {
                Title = "Export All Presets",
                FileName = $"SnapClicker_Presets_{DateTime.Now:yyyyMMdd}.json",
                DefaultExt = ".json",
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*"
            };

            if (sfd.ShowDialog() == true)
            {
                var options = new JsonSerializerOptions
                {
                    WriteIndented = true,
                    Converters = { new JsonStringEnumConverter() }
                };

                var allPresets = _presetsList.Select(p => new Preset
                {
                    Name = p.Name,
                    IsRepetitive = p.IsRepetitive,
                    RepeatCount = p.RepeatCount,
                    CreatedDate = p.CreatedDate,
                    RecordedActions = p.RecordedActions.Select(a => new RecordedAction
                    {
                        Type = a.Type,
                        X = a.X,
                        Y = a.Y,
                        Key = a.Key,
                        Timestamp = a.Timestamp,
                        IsBurstMode = a.IsBurstMode
                    }).ToList()
                }).ToList();

                var json = JsonSerializer.Serialize(allPresets, options);
                await File.WriteAllTextAsync(sfd.FileName, json);

                _snackbarService.Show(
                    "Presets Exported",
                    $"Successfully exported {allPresets.Count} preset(s) to '{Path.GetFileName(sfd.FileName)}'.",
                    ControlAppearance.Success,
                    new SymbolIcon(SymbolRegular.CheckmarkCircle20),
                    TimeSpan.FromSeconds(3)
                );
            }
        }
        catch (Exception ex)
        {
            ShowErrorMessage("Export Failed", $"Could not export presets: {ex.Message}", new SymbolIcon(SymbolRegular.ErrorCircle20));
        }
    }

    [RelayCommand]
    public async Task ImportPresetsAsync()
    {
        try
        {
            var ofd = new Microsoft.Win32.OpenFileDialog
            {
                Title = "Import Presets",
                Filter = "JSON Files (*.json)|*.json|All Files (*.*)|*.*",
                Multiselect = true
            };

            if (ofd.ShowDialog() == true)
            {
                var options = new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true,
                    Converters = { new JsonStringEnumConverter() }
                };

                int importedCount = 0;
                int skippedCount = 0;
                var existingPresets = await _presetRepository.GetAllPresetsAsync();
                var registeredNames = new HashSet<string>(existingPresets.Select(p => p.Name), StringComparer.OrdinalIgnoreCase);

                foreach (var filePath in ofd.FileNames)
                {
                    var json = await File.ReadAllTextAsync(filePath);
                    List<Preset> presetsToImport = new();

                    try
                    {
                        var single = JsonSerializer.Deserialize<Preset>(json, options);
                        if (single != null && single.RecordedActions.Any())
                            presetsToImport.Add(single);
                    }
                    catch
                    {
                        var list = JsonSerializer.Deserialize<List<Preset>>(json, options);
                        if (list != null)
                            presetsToImport.AddRange(list.Where(p => p.RecordedActions.Any()));
                    }

                    foreach (var preset in presetsToImport)
                    {
                        preset.Id = 0;
                        foreach (var act in preset.RecordedActions)
                            act.Id = 0;

                        if (string.IsNullOrWhiteSpace(preset.Name))
                            preset.Name = Path.GetFileNameWithoutExtension(filePath);

                        // Check if an exact identical duplicate already exists
                        bool isExactDuplicate = existingPresets.Any(existing =>
                            string.Equals(existing.Name, preset.Name, StringComparison.OrdinalIgnoreCase) &&
                            AreActionsEqual(existing.RecordedActions, preset.RecordedActions));

                        if (isExactDuplicate)
                        {
                            skippedCount++;
                            continue;
                        }

                        // If same name exists but actions differ, make name unique
                        string baseName = preset.Name;
                        int copyIndex = 1;
                        while (registeredNames.Contains(preset.Name))
                        {
                            preset.Name = $"{baseName} ({copyIndex++})";
                        }

                        registeredNames.Add(preset.Name);
                        await _presetRepository.AddPresetAsync(preset);
                        importedCount++;
                    }
                }

                if (importedCount > 0)
                {
                    await ReloadPresets();
                    var message = skippedCount > 0
                        ? $"Imported {importedCount} preset(s) ({skippedCount} duplicate(s) skipped)."
                        : $"Successfully imported {importedCount} preset(s).";

                    _snackbarService.Show(
                        "Presets Imported",
                        message,
                        ControlAppearance.Success,
                        new SymbolIcon(SymbolRegular.CheckmarkCircle20),
                        TimeSpan.FromSeconds(4)
                    );
                }
                else if (skippedCount > 0)
                {
                    _snackbarService.Show(
                        "Already Exists",
                        $"All selected presets already exist in your library.",
                        ControlAppearance.Caution,
                        new SymbolIcon(SymbolRegular.Info20),
                        TimeSpan.FromSeconds(4)
                    );
                }
            }
        }
        catch (Exception ex)
        {
            ShowErrorMessage("Import Failed", $"Could not import presets: {ex.Message}", new SymbolIcon(SymbolRegular.ErrorCircle20));
        }
    }

    private static bool AreActionsEqual(List<RecordedAction> a, List<RecordedAction> b)
    {
        if (a.Count != b.Count) return false;
        for (int i = 0; i < a.Count; i++)
        {
            if (a[i].Type != b[i].Type || a[i].X != b[i].X || a[i].Y != b[i].Y || a[i].Key != b[i].Key || a[i].IsBurstMode != b[i].IsBurstMode)
                return false;
        }
        return true;
    }
    
    private void ShowErrorMessage(string title, string content,SymbolIcon icon ) =>
        _snackbarService.Show(title, content,ControlAppearance.Danger, icon, TimeSpan.FromSeconds(5));
    
    private async Task ReloadPresets()
    {
        if (_presetsList.Any()) _presetsList.Clear();
        await LoadPresets();
    }
    
    public void Dispose()
    {
        PresetsView?.Dispose();
        WeakReferenceMessenger.Default.Unregister<PresetSavedMessage>(this);
    }
}