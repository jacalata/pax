﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;
using Microsoft.Xna.Framework.Media; //screen capture bmp

//navigation service?
using PAX7.ViewModel;
using PAX7.Model;
using System.IO.IsolatedStorage;

namespace PAX7
{
    public partial class App : Application
    {
        private Point _mouseDownPosition;
        private DateTime _mouseDownTime;
        public enum GESTURE { DRAG, HOLD, TAP };
        // get the title of a convention as ConventionName[Convention.PAXEAST]
        public enum Convention {PAXEAST, PAXPRIME, PAXAUS};
        public static string[] ConventionName = { "PAX East", "PAX Prime", "PAX Australia" };

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
            //hard coding to PAX East here. Will be replaced by a choice page.
            // save our value to the settings
            if (IsolatedStorageSettings.ApplicationSettings.Contains("CurrentConvention") == true)
            {
                // key already exists, remove it  
                IsolatedStorageSettings.ApplicationSettings.Remove("CurrentConvention");
            }
            IsolatedStorageSettings.ApplicationSettings.Add("CurrentConvention", ConventionName[(int)Convention.PAXEAST]);


            IsolatedStorageSettings.ApplicationSettings.Save();
//            dnp.Counter.EnableMemoryCounter = true; 
            // Global handler for uncaught exceptions. 
            UnhandledException += Application_UnhandledException;

            // Show graphics profiling information while debugging.
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // Display the current frame rate counters.
                Application.Current.Host.Settings.EnableFrameRateCounter = true;

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
        }

        // Code to execute when the application is launching (eg, from Start)
        // This code will not execute when the application is reactivated
        private void Application_Launching(object sender, LaunchingEventArgs e)
        {
            StateUtilities.IsLaunching = true;
            bool bShowNotice;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue<bool>("hasUpdateAvailable", out bShowNotice);
            if (bShowNotice)
            {
                MessageBox.Show("There are schedule updates available. Go to the about page and click 'download schedule updates' to see them.", "schedule updates!", MessageBoxButton.OK);
            }          
   
        }

        // Code to execute when the application is activated (brought to foreground)
        // This code will not execute when the application is first launched
        private void Application_Activated(object sender, ActivatedEventArgs e)
        {
            StateUtilities.IsLaunching = false;
        }

        // Code to execute when the application is deactivated (sent to background)
        // This code will not execute when the application is closing
        private void Application_Deactivated(object sender, DeactivatedEventArgs e)
        {
        }

        // Code to execute when the application is closing (eg, user hit Back)
        // This code will not execute when the application is deactivated
        private void Application_Closing(object sender, ClosingEventArgs e)
        {
        }

        // Code to execute if a navigation fails
        private void RootFrame_NavigationFailed(object sender, NavigationFailedEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // A navigation has failed; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        // Code to execute on Unhandled Exceptions
        private void Application_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
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

        private void expando_Click(object sender, RoutedEventArgs e)
        {
            PAX7.Model.Event selected = (PAX7.Model.Event)((Button)e.OriginalSource).DataContext;
            selected.ShowEventDetails();
        }

        /// <summary>
        /// Implemented my own gesture recognition engine, took the code from some example online.
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

        private void onMouseUp_MenuItem(object sender, System.Windows.Input.MouseButtonEventArgs args)
        {

            string destination = ((MenuOption)((TextBlock)args.OriginalSource).DataContext).Destination.ToString();
            PhoneApplicationFrame root = (PhoneApplicationFrame)(Application.Current.RootVisual);
            if (manipulationType(args.GetPosition(null), DateTime.Now) == GESTURE.TAP)
            {
                root.Navigate(new Uri(destination, UriKind.Relative));
                  
            }

        }

        private void ContextMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
//            string a = "b";
        }

        /*
        private void btnCaptureScreen_Click(object sender, RoutedEventArgs e)
        {
            
            WriteableBitmap wb = new WriteableBitmap(LayoutRoot, null);
            MemoryStream stream = new MemoryStream((int)LayoutRoot.ActualHeight * (int)LayoutRoot.ActualWidth * 4);
            wb.SaveJpeg(stream, (int)LayoutRoot.ActualWidth, (int)LayoutRoot.ActualHeight, 0, 100);
            stream.Seek(0, 0);
            MediaLibrary ml = new MediaLibrary();
            ml.SavePicture("CaptureFileName", stream); 

        }
        */
    }
}