namespace SnapClicker.Helpers;

public class KeyToVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is Key key)
        {
            bool invert = parameter is string param && bool.TryParse(param, out bool result) && result;
            
            bool isVisible = key != Key.None;
            return (invert ? !isVisible : isVisible) ? Visibility.Visible : Visibility.Collapsed;
        }
        
        return Visibility.Collapsed;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}