using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;

namespace PAX7.View
{
    public partial class AboutPage : PhoneApplicationPage
    {
        public AboutPage()
        {
            ((App)Application.Current).appSession.tagEvent("AboutPage");
            InitializeComponent();
        }


        private void surveyButtonClick(object sender, RoutedEventArgs e)
        {
            WebBrowserTask browser = new WebBrowserTask();
            browser.Uri = new System.Uri("https://docs.google.com/spreadsheet/viewform?formkey=dGJGV2s4MDhxazBUYlpacUFkYTNRYWc6MQ");
            browser.Show();
        }

        private void rateButtonClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Phone.Tasks.MarketplaceReviewTask task =
                new Microsoft.Phone.Tasks.MarketplaceReviewTask();
            task.Show();
        }



        private void emailButtonClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Phone.Tasks.EmailComposeTask task =
                new Microsoft.Phone.Tasks.EmailComposeTask();
            task.To = "jacalata@live.com";
            task.Subject = "Feedback: PAX Digital Assistant, Pax East 13";
            task.Show();
        }

    }
}