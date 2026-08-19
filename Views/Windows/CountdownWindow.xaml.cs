namespace SnapClicker.Views.Windows;

public partial class CountdownWindow : Window
{
    public CountdownWindowViewModel ViewModel { get; }

    public CountdownWindow(CountdownWindowViewModel viewModel)
    {
        ViewModel = viewModel;
        DataContext = this;
        InitializeComponent();

        SystemThemeWatcher.Watch(this);
    }
}
