using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data; //converter

namespace PAX7
{
    public class Converter: IValueConverter
        {
            public object Convert(object dateTime, Type objType, object something, CultureInfo culture)
            {
                DateTime date = (DateTime)dateTime;
                string daytime = date.DayOfWeek.ToString() +" " + date.TimeOfDay;
                return (object)daytime;
            }

            public object ConvertBack(object dateTime, Type objType, object something, CultureInfo culture)
            {
                MessageBox.Show("don't use this, it's just a hack");
                return DateTime.Now;
            }
        

    }
}
