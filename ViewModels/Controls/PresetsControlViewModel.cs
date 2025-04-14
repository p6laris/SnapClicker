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
    [ObservableProperty] private string _searchTerm = null!;
    
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
        WeakReferenceMessenger.Default.Register<PresetSavedMessage>(this, (_, _) => ReloadPresets());
    }
    
    [RelayCommand]
    public async Task LoadPresets()
    {
        try
        {
            var presets = await _presetRepository.GetAllPresetsAsync();

            _presetsList.Clear();
            _presetsList.AddRange(presets);
            HasPresets = _presetsList.Any();
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
        var result = await new EditPresetDialog(_dialogService.GetDialogHost(), preset).ShowAsync();
        
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
        var result = await new PresetActionEdit(_dialogService.GetDialogHost(), action).ShowAsync();
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
    
    private void ShowErrorMessage(string title, string content,SymbolIcon icon ) =>
        _snackbarService.Show(title, content,ControlAppearance.Danger, icon, TimeSpan.FromSeconds(5));
    
    private async Task ReloadPresets()
    {
        if (_presetsList.Any()) _presetsList.Clear();
        await LoadPresets();
    }
    
    public void Dispose()
    {
        WeakReferenceMessenger.Default.Unregister<PresetSavedMessage>(this);
    }
}