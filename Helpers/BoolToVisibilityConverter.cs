namespace SnapClicker.Helpers;

public class BoolToVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolValue)
        {
            bool invert = false;
            if (parameter != null)
            {
                if (parameter is bool b)
                    invert = b;
                else
                    bool.TryParse(parameter.ToString(), out invert);
            }
            return (boolValue ^ invert) ? Visibility.Visible : Visibility.Collapsed;
        }
        return Visibility.Collapsed;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Visibility visibility)
        {
            return visibility == Visibility.Visible;
        }
        return false;
    }
}