using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace DijkstraVisualization.ViewModels
{
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

    public class BoolToStatusConverter : IValueConverter
    {
        public static readonly BoolToStatusConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is bool isVisualizing)
            {
                return isVisualizing ? "Running..." : "Ready";
            }
            return "Ready";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }

    public class AnimationSpeedConverter : IValueConverter
    {
        public static readonly AnimationSpeedConverter Instance = new();

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is double interval)
            {
                return interval < 0.01 ? "Instant" : $"{interval:F1}s";
            }
            return "0.0s";
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
