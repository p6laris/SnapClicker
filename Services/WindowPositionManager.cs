namespace SnapClicker.Services;

/// <summary>
/// Provides services for calculating and adjusting window positions on screen,
/// accounting for multi-display setups, per-monitor DPI scaling, and screen boundaries.
/// </summary>
public class WindowPositionService
{
    /// <summary>
    /// Calculates the optimal window position based on screen coordinates and window dimensions across any monitor.
    /// </summary>
    public (double Left, double Top) GetCorrectWindowPosition(double x, double y, double width, double height)
    {
        if (double.IsNaN(x) || double.IsInfinity(x) || double.IsNaN(y) || double.IsInfinity(y))
        {
            return (double.NaN, double.NaN);
        }

        var pt = new PointStruct { X = (int)Math.Round(x), Y = (int)Math.Round(y) };
        IntPtr hMonitor = Methods.MonitorFromPoint(pt, MonitorNativeConstants.MONITOR_DEFAULTTONEAREST);

        double dpiScaleX = 1.0;
        double dpiScaleY = 1.0;

        if (hMonitor != IntPtr.Zero)
        {
            try
            {
                if (Methods.GetDpiForMonitor(hMonitor, MonitorNativeConstants.MDT_EFFECTIVE_DPI, out uint dpiX, out uint dpiY) == 0)
                {
                    if (dpiX > 0) dpiScaleX = dpiX / 96.0;
                    if (dpiY > 0) dpiScaleY = dpiY / 96.0;
                }
            }
            catch
            {
                dpiScaleX = GetFallbackDpiScale();
                dpiScaleY = dpiScaleX;
            }
        }
        else
        {
            dpiScaleX = GetFallbackDpiScale();
            dpiScaleY = dpiScaleX;
        }

        // Convert cursor position from physical pixels to WPF DIPs
        double scaledX = x / dpiScaleX;
        double scaledY = y / dpiScaleY;

        double monitorWorkLeft = 0;
        double monitorWorkTop = 0;
        double monitorWorkRight = SystemParameters.PrimaryScreenWidth;
        double monitorWorkBottom = SystemParameters.PrimaryScreenHeight;

        if (hMonitor != IntPtr.Zero)
        {
            var mi = new MONITORINFO { cbSize = (uint)Marshal.SizeOf<MONITORINFO>() };
            if (Methods.GetMonitorInfo(hMonitor, ref mi))
            {
                monitorWorkLeft = mi.rcWork.Left / dpiScaleX;
                monitorWorkTop = mi.rcWork.Top / dpiScaleY;
                monitorWorkRight = mi.rcWork.Right / dpiScaleX;
                monitorWorkBottom = mi.rcWork.Bottom / dpiScaleY;
            }
        }

        double newLeft = scaledX - (width / 2.0);
        double newTop = scaledY + 20;

        // Ensure window stays completely within the current monitor's work area
        if (newLeft < monitorWorkLeft)
            newLeft = monitorWorkLeft + 10;
        else if (newLeft + width > monitorWorkRight)
            newLeft = monitorWorkRight - width - 10;

        if (newTop + height > monitorWorkBottom)
            newTop = scaledY - height - 10;
        if (newTop < monitorWorkTop)
            newTop = monitorWorkTop + 10;

        return (newLeft, newTop);
    }

    private static double GetFallbackDpiScale()
    {
        var wnd = Application.Current?.MainWindow;
        if (wnd != null)
        {
            try
            {
                var dpi = VisualTreeHelper.GetDpi(wnd);
                if (dpi.DpiScaleX > 0)
                    return dpi.DpiScaleX;
            }
            catch
            {
                // ignore
            }
        }
        return 1.0;
    }
}
