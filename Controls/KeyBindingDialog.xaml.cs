namespace SnapClicker.Controls;

public partial class KeyBindingDialog : ContentDialog 
{
    public KeyBindingDialogViewModel ViewModel { get; }
    public KeyBindingDialog(ContentPresenter? presenter) : base(presenter)
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<KeyBindingDialogViewModel>();
        DataContext = this;
        
        Closed += OnClosed;
        Loaded += OnLoaded;
        
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        ViewModel.StartTracking();
    }

    private void OnClosed(ContentDialog sender, ContentDialogClosedEventArgs args)
    {
        ViewModel.StopTracking();
    }
}