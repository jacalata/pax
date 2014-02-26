using System;
using System.Collections.Generic;
using System.Windows;
using Microsoft.Phone.Controls;
using PAX7.Model; //schedule to prompt an update
using PAX7.Utilicode; //settings

namespace PAX7.View
{
    public partial class SettingsPage : PhoneApplicationPage
    {

        private bool _allowUpdate;
        private bool AllowAutoUpdate
        {
            get
            {
                return _allowUpdate;
            }
            set
            {
                Dictionary<String, String> attributes = new Dictionary<string, string>();
                attributes.Add("bool", value.ToString());
                ((App)Application.Current).appSession.tagEvent("AllowUpdateCheck", attributes);
                _allowUpdate = value;
                IsoStoreSettings.SetAutoCheckForUpdates(value);
            }
        }

        private bool _conventionAskEveryTime;
        private bool ConventionAskEveryTime
        {
            get
            {
                return _conventionAskEveryTime;
            }
            set
            {
                Dictionary<String, String> attributes = new Dictionary<string, string>();
                attributes.Add("boolDoAsk", value.ToString());
                ((App)Application.Current).appSession.tagEvent("ScheduleCheckFail", attributes);
                _conventionAskEveryTime = value;
                IsoStoreSettings.SetAutoCheckForUpdates(value);
            }
        }


        private bool _setReminders;
        private bool SetReminders
        {
            get
            {
                return _setReminders;
            }
            set
            {
                Dictionary<String, String> attributes = new Dictionary<string, string>();
                attributes.Add("bool", value.ToString());
                ((App)Application.Current).appSession.tagEvent("AllowRemindersChanged", attributes);
                _setReminders = value;
                IsoStoreSettings.SetAllowReminders(value);
            }
        }

        private Schedule schedule;

        public SettingsPage()
        {
            ((App)Application.Current).appSession.tagEvent("SettingsPage");
            InitializeComponent();
            Loaded += PageLoaded;
        }

        
        /// <summary>
        /// set the data context of the page 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PageLoaded(object sender, RoutedEventArgs e)
        {
            _allowUpdate = IsoStoreSettings.IsAllowedAutoCheckForUpdates();
            _setReminders =  IsoStoreSettings.IsAllowedSetReminders();
            string lastScheduleUpdate = IsoStoreSettings.GetLastUpdatedTime();
            TextBlock_scheduleUpdateTime.Text = lastScheduleUpdate;
            int scheduleVersion = IsoStoreSettings.GetScheduleVersion();
            TextBlock_scheduleVersion.Text = scheduleVersion.ToString();
            schedule = new Schedule();
            schedule.evt_updateCheckComplete +=
                new EventHandler(askUserToUpdate);
        }


        private void refreshScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            ((App)Application.Current).appSession.tagEvent("ClickRefreshSchedule");
            schedule.checkForNewSchedule();
        }


        private void askUserToUpdate(object sender, EventArgs e)
        {
            MessageBoxResult messageResult;
            // check the version available on the websites 
            // put up a message box telling them what I'm going to do
            if (true == IsoStoreSettings.UpdateCheckFailed())
            {
                ((App)Application.Current).appSession.tagEvent("ScheduleCheckFail");
                MessageBox.Show("Couldn't check for updates right now - try again later", "Update check failed", MessageBoxButton.OK);
            }
            else if (false == IsoStoreSettings.HasUpdateAvailable())
            {
                ((App)Application.Current).appSession.tagEvent("ScheduleCheckUpToDate");
                messageResult = MessageBox.Show("There's no new data for you right now",
                    "You're up to date!",
                    MessageBoxButton.OK);
            }
            else
            {
                ((App)Application.Current).appSession.tagEvent("ScheduleCheckWin");
                messageResult = MessageBox.Show("There's new schedule data on the web! Download it now?",
                    "Updates available!",
                    MessageBoxButton.OKCancel);

                if (messageResult == MessageBoxResult.OK)
                {
                    ((App)Application.Current).appSession.tagEvent("ScheduleCheckWinDownload");
                    // if they click ok, go ahead and download the new xml
                    schedule.DownloadNewEventFiles();
                }
            }
        }

        private void changeConventionChoiceButton_Click(object sender, RoutedEventArgs e)
        {
            App.NavigateTo("/View/LaunchPage.xaml?Settings");
        }

    }
}