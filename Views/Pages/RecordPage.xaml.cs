namespace SnapClicker.Views.Pages;

public partial class RecordPage : INavigableView<RecordPageViewModel>, INavigationAware
{
    public RecordPageViewModel ViewModel { get; }
    public RecordPage(RecordPageViewModel viewModel)
    {
        InitializeComponent();
        ViewModel = viewModel;
        DataContext = this;
    }

    public Task OnNavigatedToAsync()
    {
        return Task.CompletedTask;
    }

    public Task OnNavigatedFromAsync()
    {
        //Send a message to show the record page on the navigation pane.
        WeakReferenceMessenger.Default.Send(new RecordPageNavigatedMessage(false));
        return Task.CompletedTask;
    }
}