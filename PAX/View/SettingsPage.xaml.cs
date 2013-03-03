using System;
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
                _allowUpdate = value;
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
                _setReminders = value;
                IsoStoreSettings.SetAllowReminders(value);
            }
        }

        private Schedule schedule;

        public SettingsPage()
        {
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
            string scheduleCreationTime = IsoStoreSettings.GetScheduleCreationTime();
            TextBlock_scheduleCreationTime.Text = scheduleCreationTime;
            schedule = new Schedule();
            schedule.evt_updateCheckComplete +=
                new EventHandler(askUserToUpdate);
        }


        private void refreshScheduleButton_Click(object sender, RoutedEventArgs e)
        {
            schedule.checkForNewSchedule();
        }


        private void askUserToUpdate(object sender, EventArgs e)
        {
            MessageBoxResult messageResult;
            // check the version available on the websites 
            // put up a message box telling them what I'm going to do
            if (false == IsoStoreSettings.HasUpdateAvailable())
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