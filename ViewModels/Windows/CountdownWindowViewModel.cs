namespace SnapClicker.ViewModels.Windows;

public partial class CountdownWindowViewModel : ObservableObject
{
    [ObservableProperty] private string _countdownText = "3";
    [ObservableProperty] private string _subText = "Starting playback...";
}
