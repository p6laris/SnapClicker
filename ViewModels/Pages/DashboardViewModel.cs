namespace SnapClicker.ViewModels.Pages
{
    public partial class DashboardViewModel : ObservableObject
    {
        [ObservableProperty] private bool _hasPresets;
        [ObservableProperty] private bool _isRecordingSet = true;
    }
}
