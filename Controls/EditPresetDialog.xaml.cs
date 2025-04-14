namespace SnapClicker.Controls;

public partial class EditPresetDialog : ContentDialog 
{
    public Preset Preset { get; }
    public EditPresetDialog(ContentPresenter? presenter, Preset preset) : base(presenter)
    {
        InitializeComponent();
        Preset = preset;
        DataContext = this;
    }
}