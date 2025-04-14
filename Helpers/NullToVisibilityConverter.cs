namespace SnapClicker.Helpers
{
    public class NullToVisibilityConverter : IValueConverter
    {
        public object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            bool invert = bool.TryParse(parameter?.ToString(), out bool parsed) && parsed;
            bool isNotNull = value is not null;

            return (isNotNull ^ invert) ? Visibility.Visible : Visibility.Collapsed;
        }

        public object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            return value is Visibility visibility && visibility == Visibility.Visible;
        }
    }
}
