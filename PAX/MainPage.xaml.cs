using System;
using System.Collections.ObjectModel;
using System.IO.IsolatedStorage;
using System.Windows.Controls;
using Coding4Fun.Phone.Controls;
using Microsoft.Phone.Controls;
using PAX7.Utilicode; //menuOption


namespace PAX7
{

    public partial class MainPage : PhoneApplicationPage
    {
        public ObservableCollection<MenuOption> menuOptions;
        AboutPrompt aboutApp;
        public string twitterListURL = "http://twitter.com/jacalata/lists/pax-info";
        const bool runUnitTests = false;
        private string conventionTitle = "convention"; 
     
        // Constructor
        public MainPage()
        {
            
            InitializeComponent();
                
            MainPanorama.Title = IsolatedStorageSettings.ApplicationSettings["CurrentConvention"];
            MenuOption option1 = new MenuOption("events by day", "/View/SchedulePivotView.xaml?PivotOn=Day");
            MenuOption option2 = new MenuOption("events by location", "/View/SchedulePivotView.xaml?PivotOn=Location");
            MenuOption option4 = new MenuOption("my schedule", "/View/SchedulePivotView.xaml?PivotOn=Stars");
            MenuOption option5 = new MenuOption("search", "/View/SchedulePivotView.xaml?PivotOn=Search");
          //todo put back when expo list is up  MenuOption expo = new MenuOption("exhibitor list", "/View/SchedulePivotView.xaml?PivotOn=Expo");
            
            menuOptions = new ObservableCollection<MenuOption>{option1, option2, option4, option5};//, expo};          
            this.scheduleMenu.ItemsSource = menuOptions;
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