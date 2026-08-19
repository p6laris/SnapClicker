using System.Windows.Controls;
using SnapClicker.Models;
using Wpf.Ui.Controls;

namespace SnapClicker.Controls;

public partial class SaveDialog : ContentDialog
{
    public Preset Preset { get; }
    public SaveDialog(ContentDialogHost? host, Preset preset) : base(host)
    {
        InitializeComponent();
        DataContext = this;
        Preset = preset;
    }
}