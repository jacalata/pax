using System;
using System.Windows;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Tasks;
using PAX7.Utilicode;

namespace PAX7.View
{
    public partial class AboutPage : PhoneApplicationPage
    {
        public AboutPage()
        {
            InitializeComponent();
            AnalyticsTracker tracker = new AnalyticsTracker();
            tracker.Track("AboutPage", "Loaded");
        }


        private void surveyButtonClick(object sender, RoutedEventArgs e)
        {
            WebBrowserTask browser = new WebBrowserTask();
            browser.Uri = new System.Uri("https://docs.google.com/spreadsheet/viewform?formkey=dGJGV2s4MDhxazBUYlpacUFkYTNRYWc6MQ");
            browser.Show();
        }

        private void rateButtonClick(object sender, RoutedEventArgs e)
        {
            MarketplaceReviewTask task = new MarketplaceReviewTask();
            task.Show();
        }

    
        private void emailButtonClick(object sender, RoutedEventArgs e)
        {
            EmailComposeTask task = new EmailComposeTask();
            task.To = "jacalata@live.com";
            task.Subject = "Feedback: PAX Digital Assistant, PAX South 15";
            task.Show();
        }

    }
}