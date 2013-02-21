using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data; //converter

namespace PAX7
{
    /*
    //this class isn't used anywhere in the app
    public class DateToStringConverter : IValueConverter
        {
            public object Convert(object dateTime, Type objType, object unused, CultureInfo culture)
            {
                DateTime date = (DateTime)dateTime;
                string daytime = date.DayOfWeek.ToString() +" " + date.TimeOfDay;
                return (object)daytime;
            }

            public object ConvertBack(object dateTime, Type objType, object unused, CultureInfo culture)
            {
                MessageBox.Show("don't use this, it's just a hack");
                return DateTime.Now;
            }
        

    }
     * */

    /// <summary>
    /// A generic boolean to object converter from 
    /// http://geekswithblogs.net/codingbloke/archive/2010/05/28/a-generic-boolean-value-converter.aspx
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public class BoolToValueConverter<T> : IValueConverter
    {
        public T FalseValue { get; set; }
        public T TrueValue { get; set; }

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value == null)
                return FalseValue;
            else
                return (bool)value ? TrueValue : FalseValue;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            return value != null ? value.Equals(TrueValue) : false;
        }
    }

    /// <summary>
    /// specific instance of the generic bool converter
    /// used to make the 'selected' image visible on an event listing only if starred=true
    /// </summary>
    public class BoolToVisibilityConverter : BoolToValueConverter<Visibility> { }

}
