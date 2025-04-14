namespace SnapClicker.Controls;

public partial class PresetActionEdit : ContentDialog 
{
    public PresetActionEditViewModel ViewModel { get; }
    public PresetActionEdit(ContentPresenter? contentPresenter, RecordedAction action) : base(contentPresenter)
    {
        InitializeComponent();
        ViewModel = App.Services.GetRequiredService<PresetActionEditViewModel>();
        
        ViewModel.CursorX = action.X;
        ViewModel.CursorY = action.Y;
        ViewModel.Key = action.Key;
        ViewModel.ActionType = action.Type;
        
        DataContext = this;
    }
}