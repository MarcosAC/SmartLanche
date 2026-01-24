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
                    OrderStatus.InPreparation => new SolidColorBrush(Colors.OrangeRed),
                    OrderStatus.Ready => new SolidColorBrush(Colors.MediumSeaGreen),
                    OrderStatus.Cancelled => new SolidColorBrush(Colors.DarkRed),
                    _ => new SolidColorBrush(Colors.Gray)
                };
            }

            return new SolidColorBrush(Colors.Transparent);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
