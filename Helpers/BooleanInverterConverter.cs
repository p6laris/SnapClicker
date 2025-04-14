namespace SnapClicker.Helpers;

public class BooleanInverterConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is bool boolean)
            return !boolean;

        return value;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if(value is bool boolean)
            return !boolean;
        
        return value;
    }
}