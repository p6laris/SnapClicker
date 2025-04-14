namespace SnapClicker.Services;

/// <summary>
/// Provides services for calculating and adjusting window positions on screen,
/// accounting for DPI scaling and screen boundaries.
/// </summary>
public class WindowPositionService
{
    private static readonly double DpiScale;

    /// <summary>
    /// Static constructor that initializes the DPI scale factor.
    /// </summary>
    static WindowPositionService()
    {
        DpiScale = GetDpiScale();
    }

    /// <summary>
    /// Calculates the optimal window position based on screen coordinates and window dimensions.
    /// </summary>
    /// <param name="x">The X coordinate of the reference point (usually cursor position).</param>
    /// <param name="y">The Y coordinate of the reference point (usually cursor position).</param>
    /// <param name="width">The width of the window to position.</param>
    /// <param name="height">The height of the window to position.</param>
    /// <returns>
    /// A tuple containing the calculated (Left, Top) position for the window.
    /// Returns (NaN, NaN) if the input coordinates are invalid.
    /// </returns>
    /// <remarks>
    /// This method:
    /// - Adjusts for DPI scaling
    /// - Ensures the window stays within screen bounds
    /// - Positions the window slightly offset from the reference point
    /// </remarks>
    public (double Left, double Top) GetCorrectWindowPosition(double x, double y, double width, double height)
    {
        double scaledX = x / DpiScale;
        double scaledY = y / DpiScale;

        if (double.IsNaN(scaledX) || double.IsInfinity(scaledX) ||
            double.IsNaN(scaledY) || double.IsInfinity(scaledY))
        {
            return (double.NaN, double.NaN); // Invalid position
        }

        double screenWidth = SystemParameters.PrimaryScreenWidth;
        double screenHeight = SystemParameters.PrimaryScreenHeight;

        double newLeft = scaledX - (width / 2);
        double newTop = scaledY + 20;

        // Handle window position at the edges of the screen
        if (newLeft < 0) newLeft = scaledX + 10;
        if (newLeft + width > screenWidth) newLeft = scaledX - width - 10;
        if (newTop + height > screenHeight) newTop = scaledY - height - 10;

        return (newLeft, newTop);
    }
    
    private static double GetDpiScale()
    {
        var wnd = Application.Current.MainWindow;
        var dpi = VisualTreeHelper.GetDpi(wnd);
        return dpi.DpiScaleX;
    }
}
