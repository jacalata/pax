#define DEBUG_AGENT // for periodic agent testing

using System;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Windows.Controls;
using Coding4Fun.Phone.Controls;
using Microsoft.Phone.Controls;
using PAX7.Utilicode; //menuOption
using Microsoft.Phone.Scheduler;
using System.Windows;

namespace PAX7
{

    public partial class MainPage : PhoneApplicationPage
    {
        public ObservableCollection<MenuOption> menuOptions;
        AboutPrompt aboutApp;
        public string twitterListURL = "http://twitter.com/jacalata/lists/pax-info";
        const bool runUnitTests = false;
        PeriodicTask periodicTask;
        string periodicTaskName = "PeriodicAgent";
        // Constructor
     
        // Constructor
        public MainPage()
        {            
            InitializeComponent();

            MainPanorama.Title = IsoStoreSettings.ConventionNames[IsoStoreSettings.GetDefaultConvention()];
            MenuOption option1 = new MenuOption("events by day", "/View/SchedulePivotView.xaml?PivotOn=Day");
            MenuOption option2 = new MenuOption("events by location", "/View/SchedulePivotView.xaml?PivotOn=Location");
            MenuOption option4 = new MenuOption("my schedule", "/View/SchedulePivotView.xaml?PivotOn=Stars");
            MenuOption option5 = new MenuOption("search", "/View/SchedulePivotView.xaml?PivotOn=Search");
            menuOptions = new ObservableCollection<MenuOption>{option1, option2, option4, option5};          
            this.scheduleMenu.ItemsSource = menuOptions;

            #region run periodic task for live tile update
            // Obtain a reference to the period task, if one exists
            periodicTask = ScheduledActionService.Find(periodicTaskName) as PeriodicTask;
 
            if (periodicTask != null)
            {
                RemoveAgent(periodicTaskName);
            }
 
            periodicTask = new PeriodicTask(periodicTaskName);
 
            // The description is required for periodic agents. This is the string that the user
            // will see in the background services Settings page on the device.
            periodicTask.Description = "This demonstrates a periodic task.";
 
            // Place the call to Add in a try block in case the user has disabled agents.
            try
            {
                ScheduledActionService.Add(periodicTask);
 
                // If debugging is enabled, use LaunchForTest to launch the agent in one minute.
                #if(DEBUG_AGENT)
                    ScheduledActionService.LaunchForTest(periodicTaskName, TimeSpan.FromSeconds(10));
                #endif
            }
            catch (InvalidOperationException exception)
            {
                if (exception.Message.Contains("BNS Error: The action is disabled"))
                {
                    MessageBox.Show("Background agents for this application have been disabled by the user.");
 
                }
 
                if (exception.Message.Contains("BNS Error: The maximum number of ScheduledActions of this type have already been added."))
                {
                    // No user action required. The system prompts the user when the hard limit of periodic tasks has been reached.
 
                }
 
            }
            catch (SchedulerServiceException)
            {
                // No user action required.  
            }
            #endregion


        }
        private void RemoveAgent(string name)
        {
            try
            {
                ScheduledActionService.Remove(name);
            }
            catch (Exception)
            {
            }
        }
            

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            if (this.NavigationService.CanGoBack) //I think this will only be true for the launch page? 
               this.NavigationService.RemoveBackEntry();
          //  if (IsoStoreSettings.GetOpenedCount() > 5 && IsoStoreSettings.IsoStoreShowRatingPromptAgain())
          //  {
          //      MessageBo
                
            base.OnNavigatedTo(e);
        }


        private void Me_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            aboutApp = new AboutPrompt();
            aboutApp.VersionNumber = IsoStoreSettings.GetAppVersion().ToString();
            aboutApp.Title = "PDA: " + IsoStoreSettings.ConventionNames[IsoStoreSettings.GetDefaultConvention()];
            ContentControl detailsBody = new ContentControl();
            detailsBody.DataContext = this;
            detailsBody.Template = App.Current.Resources["aboutApp"] as ControlTemplate;
            aboutApp.Body = detailsBody;
 
            aboutApp.Show();
  
        }

        private void AppBarSettings_Click(object sender, EventArgs e)
        {
            string uri = "/View/SettingsPage.xaml";
            App.NavigateTo(uri);
        }

        private void AppBarAbout_Click(object sender, EventArgs e)
        {
            string uri = "/View/AboutPage.xaml";
            App.NavigateTo(uri);
        }

        private void AppBarUpdate_Click(object sender, EventArgs e)
        {
            string uri = "/View/SettingsPage.xaml";
            App.NavigateTo(uri);

        }

    }

}