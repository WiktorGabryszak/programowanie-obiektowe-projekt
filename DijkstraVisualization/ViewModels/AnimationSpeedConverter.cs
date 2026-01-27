using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace DijkstraVisualization.ViewModels
{
    /// <summary>
    /// Converts animation interval (in seconds) to a display string.
    /// </summary>
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
