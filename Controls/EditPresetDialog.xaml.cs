namespace SnapClicker.Controls;

public partial class EditPresetDialog : ContentDialog 
{
    public Preset Preset { get; }
    public EditPresetDialog(ContentDialogHost? host, Preset preset) : base(host)
    {
        InitializeComponent();
        Preset = preset;
        DataContext = this;
    }
}