namespace SnapClicker.Helpers;

public class MouseActionsToVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ActionType action)
        {
            bool invert = parameter is string param && bool.TryParse(param, out bool result) && result;
            bool isVisible = action is ActionType.LeftMouseClick
                             || action is ActionType.RightMouseClick
                             || action is ActionType.MiddleMouseClick
                             || action is ActionType.MouseMove;
            
            return (invert ? !isVisible : isVisible) ? Visibility.Visible : Visibility.Collapsed;
        }
        
        return Visibility.Collapsed;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}