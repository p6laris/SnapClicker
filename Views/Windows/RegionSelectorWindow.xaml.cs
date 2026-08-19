namespace SnapClicker.Views.Windows;

public partial class RegionSelectorWindow : Window
{
    private bool _isSelecting;
    private Point _startPoint;

    public Rect SelectedRegion { get; private set; }

    public RegionSelectorWindow()
    {
        InitializeComponent();
        
        Left = SystemParameters.VirtualScreenLeft;
        Top = SystemParameters.VirtualScreenTop;
        Width = SystemParameters.VirtualScreenWidth;
        Height = SystemParameters.VirtualScreenHeight;

        Loaded += OnLoaded;
        MouseDown += OnMouseDown;
        MouseMove += OnMouseMove;
        MouseUp += OnMouseUp;
        KeyDown += OnKeyDown;
    }

    private void OnLoaded(object sender, RoutedEventArgs e)
    {
        Canvas.SetLeft(InstructionBorder, (Width - InstructionBorder.ActualWidth) / 2);
    }

    private void OnMouseDown(object sender, MouseButtonEventArgs e)
    {
        if (e.ChangedButton == MouseButton.Left)
        {
            _isSelecting = true;
            _startPoint = e.GetPosition(this);

            Canvas.SetLeft(SelectionBorder, _startPoint.X);
            Canvas.SetTop(SelectionBorder, _startPoint.Y);
            SelectionBorder.Width = 0;
            SelectionBorder.Height = 0;
            SelectionBorder.Visibility = Visibility.Visible;

            InfoBadge.Visibility = Visibility.Visible;
            Canvas.SetLeft(InfoBadge, _startPoint.X);
            Canvas.SetTop(InfoBadge, _startPoint.Y + 10);

            CaptureMouse();
        }
    }

    private void OnMouseMove(object sender, MouseEventArgs e)
    {
        if (!_isSelecting)
            return;

        var currentPoint = e.GetPosition(this);
        var x = Math.Min(_startPoint.X, currentPoint.X);
        var y = Math.Min(_startPoint.Y, currentPoint.Y);
        var width = Math.Abs(_startPoint.X - currentPoint.X);
        var height = Math.Abs(_startPoint.Y - currentPoint.Y);

        Canvas.SetLeft(SelectionBorder, x);
        Canvas.SetTop(SelectionBorder, y);
        SelectionBorder.Width = width;
        SelectionBorder.Height = height;

        InfoText.Text = $"X: {(int)(x + Left)}, Y: {(int)(y + Top)}  |  {(int)width} × {(int)height} px";
        
        var badgeTop = (y + height + 10 > Height - 40) ? y - 30 : y + height + 10;
        Canvas.SetLeft(InfoBadge, Math.Max(10, Math.Min(x, Width - 200)));
        Canvas.SetTop(InfoBadge, Math.Max(10, badgeTop));
    }

    private void OnMouseUp(object sender, MouseButtonEventArgs e)
    {
        if (!_isSelecting)
            return;

        _isSelecting = false;
        ReleaseMouseCapture();

        var currentPoint = e.GetPosition(this);
        var x = Math.Min(_startPoint.X, currentPoint.X);
        var y = Math.Min(_startPoint.Y, currentPoint.Y);
        var width = Math.Abs(_startPoint.X - currentPoint.X);
        var height = Math.Abs(_startPoint.Y - currentPoint.Y);

        if (width < 2 && height < 2)
        {
            SelectedRegion = new Rect(_startPoint.X + Left, _startPoint.Y + Top, 1, 1);
        }
        else
        {
            SelectedRegion = new Rect(x + Left, y + Top, width, height);
        }

        DialogResult = true;
        Close();
    }

    private void OnKeyDown(object sender, KeyEventArgs e)
    {
        if (e.Key == Key.Escape)
        {
            DialogResult = false;
            Close();
        }
    }
}
