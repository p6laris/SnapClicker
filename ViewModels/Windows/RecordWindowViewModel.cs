namespace SnapClicker.ViewModels.Windows;

public partial class RecordWindowViewModel : ObservableObject ,IDisposable
{
    private readonly IRecorderManagerService _recorderManagerService;
    
    /// <summary>
    /// On cursor position changed.
    /// </summary>
    public event Action<double,double>? OnCursorPositionChanged;
    
    [ObservableProperty] private int _cursorX;
    [ObservableProperty] private int _cursorY;
    [ObservableProperty] private bool _isKeyType;
    
    [ObservableProperty] private ActionType _type;
    [ObservableProperty] private Key _key;
    
    private readonly DispatcherTimer _timer;
    
    public RecordWindowViewModel(IRecorderManagerService recorderManagerService)
    {
        _recorderManagerService = recorderManagerService;
        _recorderManagerService.OnNewRecord += OnAddNewRecord;
        _timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(50) };
        _timer.Tick += UpdatePosition;
        _timer.Start();
        
    }

    private void OnAddNewRecord(RecordedAction record)
    {
        Key = record.Key;
        Type = record.Type;
    }

    private void UpdatePosition(object? sender, EventArgs e)
    {
        if (GetCursorPos(out POINT cursorPos))
        {
            CursorX = cursorPos.X;
            CursorY = cursorPos.Y;
            OnCursorPositionChanged?.Invoke(cursorPos.X, cursorPos.Y);
        }
    }
    
    [DllImport("user32.dll")]
    private static extern bool GetCursorPos(out POINT lpPoint);
    
    [StructLayout(LayoutKind.Sequential)]
    private struct POINT
    {
        public int X;
        public int Y;
    }

    public void Dispose()
    {
        _recorderManagerService.OnNewRecord -= OnAddNewRecord;
        _recorderManagerService.StopRecording();
    }
}