using Google.Protobuf;
using System.Globalization;
using System.Text;
using System.Windows.Data;

namespace MVPlot.Utilities
{
    public class Bytes2StringConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((ByteString)value).ToString(Encoding.Default);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return null!;
        }
    }
}
