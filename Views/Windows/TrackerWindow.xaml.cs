using System.Windows.Controls.Primitives;
using System.Windows.Media;
using SnapClicker.Services;
using SnapClicker.ViewModels.Windows;
using Wpf.Ui.Appearance;

namespace SnapClicker.Views.Windows;

public partial class TrackerWindow : Window
{
    
    private readonly WindowPositionService _windowPositionService;
    public TrackerWindowViewModel ViewModel { get; }
    public TrackerWindow(TrackerWindowViewModel viewModel, WindowPositionService windowPositionService)
    {
        InitializeComponent();
        ViewModel = viewModel;
        _windowPositionService = windowPositionService;
        
        //Change this window's position to follow the cursor (only when it's for tracking mouse)
        ViewModel.OnCursorPositionChanged += UpdateWindowPosition;
        DataContext = this;
        
        SystemThemeWatcher.Watch(this);
    }
    private void UpdateWindowPosition(double x, double y)
    {
        var (left, top) = _windowPositionService.GetCorrectWindowPosition(x, y, Width, Height);

        if (double.IsNaN(left) || double.IsNaN(top)) return;

        Dispatcher.BeginInvoke(() =>
        {
            Left = left;
            Top = top;
        });
    }
}