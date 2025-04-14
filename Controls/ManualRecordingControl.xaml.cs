namespace SnapClicker.Controls;

public partial class ManualRecordingControl : UserControl
{
    public ManualRecordingControlViewModel ViewModel { get; }
    public ManualRecordingControl()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<ManualRecordingControlViewModel>();
        DataContext = this;
    }
}