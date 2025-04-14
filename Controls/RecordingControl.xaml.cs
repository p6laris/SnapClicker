namespace SnapClicker.Controls;

public partial class RecordingControl : UserControl
{
    public RecordingControlViewModel ViewModel { get; }
    public RecordingControl()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<RecordingControlViewModel>();
        DataContext = this;
    }
}