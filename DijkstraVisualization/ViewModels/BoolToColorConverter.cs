using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace DijkstraVisualization.ViewModels
{
    /// <summary>
    /// Converts a boolean value to a color brush (LimeGreen when true, Gray when false).
    /// </summary>
    public class BoolToColorConverter : IValueConverter
    {
        public static readonly BoolToColorConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isVisualizing)
            {
                return isVisualizing 
                    ? new SolidColorBrush(Colors.LimeGreen) 
                    : new SolidColorBrush(Colors.Gray);
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
