using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PAX7.Utilicode; //settings

namespace PAX7.View
{
    public partial class LaunchPage : PhoneApplicationPage
    {
        //store the convention value at the start, so we know to re-read all the events if it changes
        int startConventionID = 0;

        public List<string> conventions { 
            get
            {
                return PAX7.Utilicode.IsoStoreSettings.getConventionNames(false);
            }
            set
            {
                //don't set anything
            }
        }
        public bool bAskEveryTime { 
            get
            {
                return IsoStoreSettings.IsAskEveryTime();
            }
            set
            {
                IsoStoreSettings.SetAskEveryTime(value);
            }
        }
   
        
        public LaunchPage()
        {
            startConventionID = IsoStoreSettings.GetDefaultConvention();
            InitializeComponent();
            conventionChoiceList.DataContext = this;
        }

        /// <summary>
        /// user has made a selection and wants to go to that convention. 
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Launch_Button_Click(object sender, RoutedEventArgs e)
        {
            App.NavigateTo("/MainPage.xaml?Choice");
        }



        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            ((App)Application.Current).appSession.tagEvent("ConventionChoicePage");
            // when launched from Settings, we'll get that in the url.
            // if so, don't show the 'GO' button - user can navigate back with the back button
            if (this.NavigationContext.QueryString.ContainsKey("Settings"))
                Launch_Button.Visibility = System.Windows.Visibility.Collapsed;
            base.OnNavigatedTo(e);
        }


        /// <summary>
        /// user is leaving the page. Save the value of 'ask me every time'
        /// Also check if the chosen convention has changed and call schedule loader if so
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedFrom(NavigationEventArgs e)
        {
            //TODO: the checkbox should be a twoway that updates the value automatically instead of this hack
            bAskEveryTime = checkAskEveryTime.IsChecked ?? true;
            IsoStoreSettings.SetAskEveryTime(bAskEveryTime);
            if (IsoStoreSettings.GetDefaultConvention() != startConventionID)
                IsoStoreSettings.SetConventionChanged(true);
            base.OnNavigatedFrom(e);
        }

    }
}