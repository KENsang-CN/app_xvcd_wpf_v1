using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Data;

namespace Converters
{
    [ValueConversion(typeof(bool), typeof(bool))]
    [ValueConversion(typeof(bool?), typeof(bool?))]
    public class InverseBooleanConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(bool?))
            {
                bool? b = (bool?)value;
                return b.HasValue && !b.Value;
            }
            else if (targetType == typeof(bool))
            {
                return !(bool)value;
            }
            else
            {
                throw new InvalidOperationException("the target must be a nullable boolean");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(bool?))
            {
                bool? b = (bool?)value;
                return b.HasValue && !b.Value;
            }
            else if (targetType == typeof(bool))
            {
                return !(bool)value;
            }
            else
            {
                throw new InvalidOperationException("the target must be a nullable boolean");
            }
        }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class BooleanVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(Visibility))
            {
                bool v = (bool)value;
                return v ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                throw new InvalidOperationException("the target must be a Visibility");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(bool))
            {
                Visibility v = (Visibility)value;
                return (v == Visibility.Visible);
            }
            else
            {
                throw new InvalidOperationException("the target must be a boolean");
            }
        }
    }

    [ValueConversion(typeof(bool), typeof(Visibility))]
    public class InverserBooleanVisibilityConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(Visibility))
            {
                bool v = !(bool)value;
                return v ? Visibility.Visible : Visibility.Collapsed;
            }
            else
            {
                throw new InvalidOperationException("the target must be a Visibility");
            }
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (targetType == typeof(bool))
            {
                Visibility v = (Visibility)value;
                return !(v == Visibility.Visible);
            }
            else
            {
                throw new InvalidOperationException("the target must be a boolean");
            }
        }
    }

    [ValueConversion(typeof(bool), typeof(object))]
    public class BooleanArrayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Array arr = (Array)parameter;
            if (arr.Length != 2)
            {
                throw new InvalidOperationException("invalid parameter length");
            }

            bool v = (bool)value;

            return v ? arr.GetValue(0) : arr.GetValue(1);
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            Array arr = (Array)parameter;
            if (arr.Length != 2)
            {
                throw new InvalidOperationException("invalid parameter length");
            }

            if (targetType == typeof(bool))
            {
                bool ret = false;

                if (arr.GetValue(0).Equals(value))
                {
                    ret = true;
                }
                else if (arr.GetValue(1).Equals(value))
                {
                    ret = false;
                }

                return ret;
            }
            else
            {
                throw new InvalidOperationException("the target must be a boolean");
            }
        }
    }

    [ValueConversion(typeof(double), typeof(string))]
    public class HumainDisplayConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            double dval = 0;
            try
            {
                dval = (double)value;
            }
            catch { };
            var hdu = new HumanDisplayUnit(dval);

            return (dval > 0) ? $"{hdu.Base} {hdu.Unit}{parameter as string}" : "自动";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            var str = (value as string).Replace(parameter as string, string.Empty);
            var hdu = (str == "自动") ? new HumanDisplayUnit(0) : new HumanDisplayUnit(str);

            return hdu.Value;
        }
    }
}
