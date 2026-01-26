using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace SmartLanche.Helpers
{
    public class StatusToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is OrderStatus status)
            {
                return status switch
                {
                    OrderStatus.InPreparation => Brushes.OrangeRed,
                    OrderStatus.Ready => Brushes.MediumSeaGreen,
                    OrderStatus.Cancelled => Brushes.DarkRed,
                    _ => Brushes.Gray
                };
            }

            return Brushes.Transparent;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
}
