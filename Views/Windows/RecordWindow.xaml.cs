namespace SnapClicker.Views.Windows;

public partial class RecordWindow : Window
{
    private readonly WindowPositionService _windowPositionService;
    public RecordWindowViewModel ViewModel { get; }
    
    public RecordWindow(RecordWindowViewModel viewModel,WindowPositionService windowPositionService)
    {
        InitializeComponent();
        ViewModel = viewModel;
        _windowPositionService = windowPositionService;
        
        //Change this window's position to follow the cursor.
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

