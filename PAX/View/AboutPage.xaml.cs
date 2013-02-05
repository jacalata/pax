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

using PAX7.Model;

namespace PAX7.View
{
    public partial class AboutPage : PhoneApplicationPage
    {
        public AboutPage()
        {
            InitializeComponent();
        }


        private void surveyButtonClick(object sender, RoutedEventArgs e)
        {
            Microsoft.Phone.Tasks.MarketplaceReviewTask task =
                new Microsoft.Phone.Tasks.MarketplaceReviewTask();
            task.Show();
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

        private void refreshScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            Schedule schedule = new Schedule();
            schedule.checkForNewSchedule();
            schedule.evt_updateCheckComplete += 
                new EventHandler(askUserToUpdate);
        }


        private void askUserToUpdate(object sender, EventArgs e)
        {
            Schedule schedule = new Schedule();
            MessageBoxResult messageResult;
            // check the version available on the websites 
            // put up a message box telling them what I'm going to do
            if (false == schedule.HasUpdateAvailable())
            {
                messageResult = MessageBox.Show("There's no new data for you right now",
                    "You're up to date!",
                    MessageBoxButton.OK);
                return;
            }
            else
            {
                messageResult = MessageBox.Show("There's new schedule data on the web! Download it now?",
                    "Updates available!",
                    MessageBoxButton.OKCancel);

                if (messageResult == MessageBoxResult.OK)
                {
                    // if they click ok, go ahead and download the new xml
                    schedule.DownloadNewEventFiles();
                }
            }
        }

    }
}