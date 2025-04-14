﻿namespace SnapClicker.Helpers;

public class KeyActionToVisibilityConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is ActionType actionType)
        {
            bool invert = parameter is string param && bool.TryParse(param, out bool result) && result;
            
            bool isVisible = actionType == ActionType.KeyDown || actionType == ActionType.KeyUp;
            return (invert ? !isVisible : isVisible) ? Visibility.Visible : Visibility.Collapsed;
        }

        return Visibility.Collapsed;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}