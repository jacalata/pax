using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;

namespace PAX7.View
{
    public partial class Search : PhoneApplicationPage
    {
        public Search()
        {
            InitializeComponent();
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            _searchText.Focus(); //focus in textbox
        }

        private void searchButtonClick(object sender, RoutedEventArgs e)
        {
            string destination = "/View/SchedulePivotView.xaml?PivotOn=Search&SearchString="+_searchText.Text;
            NavigationService.Navigate(new Uri(destination, UriKind.Relative));

        }

    }
}