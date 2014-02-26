using System;
using System.Collections.Generic; // dictionary
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;//navigation service?
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using PAX7.Utilicode; //settings
using PAX7.ViewModel; // show event

namespace PAX7
{

    public partial class App : Application
    {
        private Point _mouseDownPosition;
        private DateTime _mouseDownTime;
        public enum GESTURE { DRAG, HOLD, TAP };

        public LocalyticsSession appSession;

        /// <summary>
        /// Provides easy access to the root frame of the Phone Application.
        /// </summary>
        /// <returns>The root frame of the Phone Application.</returns>
        public PhoneApplicationFrame RootFrame { get; private set; }

        /// <summary>
        /// Constructor for the Application object.
        /// </summary>
        public App()
        {
            // TODO remove hardcode when I put in the 'choose' page
            int con = 1;
            IsoStoreSettings.SaveDefaultConvention(con);

            // dnp.Counter.EnableMemoryCounter = true; 
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                //Application.Current.Host.Settings.EnableFrameRateCounter = true;

                // Show the areas of the app that are being redrawn in each frame.
                //Application.Current.Host.Settings.EnableRedrawRegions = true;

                // Enable non-production analysis visualization mode, 
                // which shows areas of a page that are being GPU accelerated with a colored overlay.
                //Application.Current.Host.Settings.EnableCacheVisualization = true;
            }

            // Standard Silverlight initialization
            InitializeComponent();

            // Phone-specific initialization
            InitializePhoneApplication();
            
            // handle redirecting to launch choice page if needed
            RootFrame.Navigating += new NavigatingCancelEventHandler(RootFrame_Navigating);

        }

        
        // http://blogs.msdn.com/b/ptorr/archive/2010/08/28/redirecting-an-initial-navigation.aspx?wa=wsignin1.0
        /// <summary>
        /// interrupt launch of mainpage to launch interstitial convention choice page if necessary
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RootFrame_Navigating(object sender, NavigatingCancelEventArgs e)
        {/*
            // TODO: we only want to do this on first launch each time. Move to launch method?
            // Only care about intercepting the MainPage if we haven't chosen an account yet
            if (e.Uri.ToString().Contains("/MainPage.xaml")  &&  IsoStoreSettings.IsAskEveryTime()
                && !e.Uri.ToString().Contains("Choice") ) 
            {
                // Cancel current navigation and schedule the real navigation for the next tick
                // (we can't navigate immediately as that will fail; no overlapping navigations
                // are allowed)
                
                
                e.Cancel = true;
                RootFrame.Dispatcher.BeginInvoke(delegate
                {
                     string uri = "/View/LaunchPage.xaml";
                     RootFrame.Navigate(new Uri(uri, UriKind.Relative));
                });
            }
            else
            {
                return;
            }
                 * */

        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            StateUtilities.IsLaunching = true;
            LittleWatson.CheckForPreviousException();
            appSession = new LocalyticsSession("6fbc7a37fd93bb6135581e6-eecdd2f8-7f4d-11e3-9861-009c5fda0a25");
            appSession.open();
            appSession.upload();

            // why am I doing this schedule setting here? looks like leftover code from something
            PAX7.Model.Schedule schedule = new PAX7.Model.Schedule();
            if (IsoStoreSettings.IsAllowedAutoCheckForUpdates())
            {
                schedule.checkForNewSchedule();
            }
            if (IsoStoreSettings.HasUpdateAvailable())
            {
                MessageBox.Show("Go to the settings page to download them now", "New schedule data available", MessageBoxButton.OK);
            }
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            StateUtilities.IsLaunching = false;
            appSession = new LocalyticsSession("6fbc7a37fd93bb6135581e6-eecdd2f8-7f4d-11e3-9861-009c5fda0a25");
            appSession.open();
            appSession.upload();
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
            appSession.close();
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
            appSession.close();
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            LittleWatson.ReportException(e.Exception);
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            Dictionary<String, String> attributes = new Dictionary<string, string>();
            attributes.Add("exception", e.ExceptionObject.Message);
            appSession.tagEvent("App crash", attributes);
            LittleWatson.ReportException(e.ExceptionObject);
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        #region Phone application initialization

        // Avoid double-initialization
        private bool phoneApplicationInitialized = false;

        // Do not add any additional code to this method
        private void InitializePhoneApplication()
        {
            if (phoneApplicationInitialized)
                return;

            // Create the frame but don't set it as RootVisual yet; this allows the splash
            // screen to remain active until the application is ready to render.
            RootFrame = new PhoneApplicationFrame();
            RootFrame.Navigated += CompleteInitializePhoneApplication;

            // Handle navigation failures
            RootFrame.NavigationFailed += RootFrame_NavigationFailed;

            // Ensure we don't initialize again
            phoneApplicationInitialized = true;
        }

        // Do not add any additional code to this method
        private void CompleteInitializePhoneApplication(object sender, NavigationEventArgs e)
        {
            // Set the root visual to allow the application to render
            if (RootVisual != RootFrame)
                RootVisual = RootFrame;

            // Remove this handler since it is no longer needed
            RootFrame.Navigated -= CompleteInitializePhoneApplication;
        }

        #endregion
        

        #region gesture handling code
        /// <summary>
        /// Implemented my own gesture recognition engine, took the code from some example online.
        /// There must be a better way than doing this, the tap recognition on the menu is awful
        /// </summary>
        /// <param name="mouseUpPosition"></param>
        /// <param name="mouseUpTime"></param>
        /// <returns></returns>
        public GESTURE manipulationType(Point mouseUpPosition, DateTime mouseUpTime)
        {
            int _holdTime = (mouseUpTime.AddSeconds(2).CompareTo(_mouseDownTime));
            double _deltaX = mouseUpPosition.X % _mouseDownPosition.X;
            double _deltaY = mouseUpPosition.Y % _mouseDownPosition.Y;
            if ((_deltaX > 5) && (_deltaY > 5))
            {
                return GESTURE.DRAG;
            }
            else if (_holdTime <= 0)
            {
                return GESTURE.HOLD;
            }
            else
            {
                return GESTURE.TAP;
            }

        }

        public void onMouseLeftButtonDown_MenuItem(object sender, System.Windows.Input.MouseButtonEventArgs args)
        {
            _mouseDownPosition = args.GetPosition(null);
            _mouseDownTime = DateTime.Now;
        }

        private void onMouseLeftButtonUp_MenuItem(object sender, System.Windows.Input.MouseButtonEventArgs args)
        {
            if (manipulationType(args.GetPosition(null), DateTime.Now) == GESTURE.TAP)
            {
                string destination = ((MenuOption)((TextBlock)args.OriginalSource).DataContext).Destination.ToString();
                NavigateTo(destination);
            }
        }

#endregion 
        
        
        /// <summary>
        /// Open the event details view. 
        /// Query to self: why did I put this in app.xaml.cs and not scheduleviewmodel.cs where it clearly belongs?
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void expando_Click(object sender, RoutedEventArgs e)
        {
            PAX7.Model.Event selected = (PAX7.Model.Event)((Button)e.OriginalSource).DataContext;
            EventView.ShowEventDetails(selected);
        }

        /// <summary>
        /// helper method to navigate within the app
        /// </summary>
        /// <param name="destination"></param>
        public static void NavigateTo(string destination)
        {
            PhoneApplicationFrame root = (PhoneApplicationFrame)(Application.Current.RootVisual);
            root.Navigate(new Uri(destination, UriKind.Relative));
        }

        /// <summary>
        /// radio button method for datatemplate used in LaunchPage
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void RadioButton_Checked(object sender, RoutedEventArgs e)
        {
            // how do I find out the index of the radio button from itself? look up the name?
            RadioButton rb = (RadioButton)e.OriginalSource;
            foreach (string con in IsoStoreSettings.ConventionNames)
            {
                if (rb.Content.Equals(con))
                {
                    int conventionId = IsoStoreSettings.ConventionNames.IndexOf(con);
                    IsoStoreSettings.SaveDefaultConvention(conventionId);
                    break;
                }
            }

        }

    }
}