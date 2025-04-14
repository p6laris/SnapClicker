namespace SnapClicker.Controls;

public partial class PresetsView : UserControl
{
    public PresetsControlViewModel ViewModel { get; }
    public PresetsView()
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<PresetsControlViewModel>();
        DataContext = this;
        this.Loaded += OnLoaded;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        //When the user control loaded, load the presets from the data source
        if(ViewModel.LoadPresetsCommand.CanExecute(null))
            ViewModel.LoadPresetsCommand.Execute(null);
    }
}