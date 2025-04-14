namespace SnapClicker.Controls;

public partial class PresetsItemView : UserControl
{
    public PresetsControlViewModel ViewModel { get; }
    public PresetsItemView()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<PresetsControlViewModel>();
        DataContext = this;
        DataContext = this;
    }
}