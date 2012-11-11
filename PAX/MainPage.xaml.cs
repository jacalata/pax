using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net;
using System.Xml;
using System.Xml.Linq;
using System.Windows;
using System.Windows.Controls;

using Microsoft.Phone;
using Microsoft.Phone.Controls;
using Microsoft.Silverlight.Testing;
using Coding4Fun.Phone.Controls;
using PAX7.Model;


namespace PAX7
{

    public partial class MainPage : PhoneApplicationPage
    {
        public ObservableCollection<MenuOption> menuOptions;
        AboutPrompt aboutApp;
        public string twitterListURL = "http://twitter.com/jacalata/lists/pax-info";

        const bool runUnitTests = false;

        // Constructor
        public MainPage()
        {
            InitializeComponent();
                
            MenuOption option1 = new MenuOption("events by day", "/View/SchedulePivotView.xaml?PivotOn=Day");
            MenuOption option2 = new MenuOption("events by location", "/View/SchedulePivotView.xaml?PivotOn=Location");
            MenuOption option3 = new MenuOption("events by type", "/View/SchedulePivotView.xaml?PivotOn=EventType");
            MenuOption option4 = new MenuOption("my schedule", "/View/SchedulePivotView.xaml?PivotOn=Stars");
            MenuOption option5 = new MenuOption("search", "/View/SchedulePivotView.xaml?PivotOn=Search");
            MenuOption option6 = new MenuOption("about+survey", "/View/AboutPage.xaml");
            menuOptions = new ObservableCollection<MenuOption>{option1, option2, option3, option4, option5, option6};          
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
            aboutApp.Title = "PDA, PAX Prime 2011";
            ContentControl detailsBody = new ContentControl();
            detailsBody.DataContext = this;
            detailsBody.Template = App.Current.Resources["aboutApp"] as ControlTemplate;
            aboutApp.Body = detailsBody;
 
            aboutApp.Show();
  
        }

        private void getTwitterButton_Click(object sender, RoutedEventArgs e)
        {
            Microsoft.Phone.Tasks.MarketplaceDetailTask twitterApp = new Microsoft.Phone.Tasks.MarketplaceDetailTask();
            twitterApp.ContentIdentifier = "0b792c7c-14dc-df11-a844-00237de2db9e";
            twitterApp.ContentType = Microsoft.Phone.Tasks.MarketplaceContentType.Applications;
            twitterApp.Show();
        }
        

    }

}