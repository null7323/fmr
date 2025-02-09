using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace FMR.Core.UI
{
#pragma warning disable CS1591
    public class NotValueConverter : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !Convert.ToBoolean(value);
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return !Convert.ToBoolean(value);
        }
    }

    public class AndValueConverter : IMultiValueConverter
    {
        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (object value in values)
            {
                bool convertedValue = Convert.ToBoolean(value);
                if (!convertedValue)
                {
                    return false;
                }
            }
            return true;
        }

        object[]? IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            ThrowHelper.ThrowNotImplemented();
            return null;
        }
    }

    public class OrValueConverter : IMultiValueConverter
    {
        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            foreach (object value in values)
            {
                bool convertedValue = Convert.ToBoolean(value);
                if (convertedValue)
                {
                    return true;
                }
            }
            return false;
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            ThrowHelper.ThrowNotImplemented();
            return null;
        }
    }
#pragma warning restore CS1591
}
