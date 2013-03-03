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
        private ObservableCollection<MenuOption> menuOptions;
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
            MenuOption option3 = new MenuOption("events by type", "/View/SchedulePivotView.xaml?PivotOn=EventType");
            MenuOption option4 = new MenuOption("my schedule", "/View/SchedulePivotView.xaml?PivotOn=Stars");
            MenuOption option5 = new MenuOption("search", "/View/SchedulePivotView.xaml?PivotOn=Search");
            menuOptions = new ObservableCollection<MenuOption>{option1, option2, option3, option4, option5};          
            this.scheduleMenu.ItemsSource = menuOptions;
        }
            

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }


        private void Me_ManipulationCompleted(object sender, System.Windows.Input.ManipulationCompletedEventArgs e)
        {
            aboutApp = new AboutPrompt();
            aboutApp.VersionNumber = "1.5";
            aboutApp.Title = "PDA: " + conventionTitle;
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