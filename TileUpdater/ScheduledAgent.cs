#define DEBUG_AGENT

using System.Diagnostics;
using System.Windows;
using Microsoft.Phone.Scheduler;
using Microsoft.Phone.Shell;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel; // observable collection
using System.Windows.Media.Imaging;  //bitmap
using System.Windows.Media; // solidcolorbrush
using Telerik.Windows.Controls; //LiveTileHelper?;
using System.Windows.Controls; // text block, Image
using System.IO.IsolatedStorage;
using PAX7.Model;

namespace TileUpdater
{
    public class ScheduledAgent : ScheduledTaskAgent
    {
        private static volatile bool _classInitialized;
        private string mediumTileFilename = "/Shared/ShellContent/PAXTileMedium.jpg";
        private string mediumTileBackFilename = "/Shared/ShellContent/PAXTileMediumBack.jpg";
        private string wideTileFilename = "/Shared/ShellContent/PAXTileWide.jpg";
        private string wideTileBackFilename = "/Shared/ShellContent/PAXTileWideBack.jpg";
        private string smallTileFilename = "/Shared/ShellContent/PAXTileSmall.jpg";

        /// <remarks>
        /// ScheduledAgent constructor, initializes the UnhandledException handler
        /// </remarks>
        public ScheduledAgent()
        {
            if (!_classInitialized)
            {
                _classInitialized = true;
                // Subscribe to the managed exception handler
                Deployment.Current.Dispatcher.BeginInvoke(delegate
                {
                    Application.Current.UnhandledException += ScheduledAgent_UnhandledException;
                });
            }
        }

        /// Code to execute on Unhandled Exceptions
        private void ScheduledAgent_UnhandledException(object sender, ApplicationUnhandledExceptionEventArgs e)
        {
            if (System.Diagnostics.Debugger.IsAttached)
            {
                // An unhandled exception has occurred; break into the debugger
                System.Diagnostics.Debugger.Break();
            }
        }

        private int getTileHeight(bool isWide)
        {
            /*
            if (App.Current.Host.Content.ScaleFactor == 100)
            {
                return 210; // WVGA
            }
            else */
            return 336; 
            //if I add small, then it is 159 or 100 for WVGA
        }

        private int getTileWidth(bool isWide)
        {
            if (isWide)
                return 691; // TODO for WVGA = 430
            else
                return 336; //TODO for WVGA = 210
            //if I add small, then it is 159 or 100 for WVGA
        }

        private delegate void DelegateObject(int width, int height);


        /// <summary>
        /// Agent that runs a scheduled task
        /// </summary>
        /// <param name="task">
        /// The invoked task
        /// </param>
        /// <remarks>
        /// This method is called when a periodic or resource intensive task is invoked
        /// </remarks>
        protected override void OnInvoke(ScheduledTask task)
        {
            /* proof of background agent life code 
            string toastMessage = "here is a toast from scheduled task";
            // Launch a toast to show that the agent is running.
            // The toast will not be shown if the foreground application is running.
            ShellToast toast = new ShellToast();
            toast.Title = "Scheduled Task is running!";
            toast.Content = toastMessage;
            toast.Show();
            */

            // get application tile
            var tiles = ShellTile.ActiveTiles.GetEnumerator();
            while (tiles.Current == null)
                tiles.MoveNext(); 
            ShellTile tile = tiles.Current;
           // ShellTile tile = LiveTileHelper.GetTile(null);
            if (null != tile)
            {
                //TODO: generate image with next scheduled session  
                //see http://stackoverflow.com/questions/7377223/wp7-generating-tile-image-cannot-include-my-own-image
                Deployment.Current.Dispatcher.BeginInvoke(GenerateImage);
                Deployment.Current.Dispatcher.BeginInvoke(setSmallImage);
                if (LiveTileHelper.AreNewTilesSupported)
                {
                    FlipTileData flipTile = new FlipTileData();
                  
                    flipTile.BackgroundImage = new Uri("isostore:" + mediumTileFilename, UriKind.Absolute);
                    flipTile.WideBackgroundImage = new Uri("isostore:" + wideTileFilename, UriKind.Absolute);
                    flipTile.SmallBackgroundImage = new Uri("isostore:" + smallTileFilename, UriKind.Absolute);

                    flipTile.BackBackgroundImage = new Uri("isostore:" + mediumTileBackFilename, UriKind.Absolute);
                    flipTile.WideBackBackgroundImage = new Uri("isostore:" + wideTileBackFilename, UriKind.Absolute);

                    tile.Update(flipTile);

                    // if I wanted to choose a newer tile type for newer phones, I'd use this
                    // LiveTileHelper.CreateOrUpdateTile(flipTile, new Uri("http://google.com"), LiveTileHelper.AreNewTilesSupported);
                }
                else
                {
                    RadExtendedTileData tileData = new RadExtendedTileData();
                    tileData.BackgroundImage = new Uri("isostore:" + mediumTileFilename, UriKind.Absolute);
                    tileData.BackBackgroundImage = new Uri("isostore:" + mediumTileBackFilename, UriKind.Absolute);
                    tile.Update(tileData);
                }               
            }

            #if DEBUG_AGENT
                ScheduledActionService.LaunchForTest(task.Name, TimeSpan.FromSeconds(60));
            #endif
            NotifyComplete();
            
        }

        private void setSmallImage()
        {
            var source = new BitmapImage();
            source.CreateOptions = BitmapCreateOptions.None;
            Uri imageUri = new Uri("Images/Icons/logoCrossTransparent.png", UriKind.Relative);
            System.Windows.Resources.StreamResourceInfo s = Application.GetResourceStream(imageUri);
            source.SetSource(s.Stream);
            var b = new WriteableBitmap(source);
            
            var background = new Canvas();
            background.Height = b.PixelHeight;
            background.Width = b.PixelWidth;
            
            b.Render(background, null);
            b.Invalidate(); //Draw bitmap

            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                using (var st = new IsolatedStorageFileStream(smallTileFilename, System.IO.FileMode.Create, System.IO.FileAccess.Write, store))
                {
                    b.SaveJpeg(st, b.PixelWidth, b.PixelHeight, 0, 100);
                }
            }
        }

        private ObservableCollection<Event> fetchNextEvents(bool onlyNextHour)
        {
            Schedule schedule = new Schedule();
            string myQuery = "";
            return null; // schedule.sqlSelectStarredEvents();
         /*   if (!onlyNextHour)
            {
                // get all events that start at the next start time listed
                string afterNow = "select * from table where start time > Now group by start time ";
                string min = "select min(starttime) from afterNow";
                string getAll = "select * from table where starttime = min";
            }
            else
            {
                myQuery = "select * from table where (starttime > Now) and (starttime < Now + 1 hour";
            }
            schedule.db.SelectList<Event>(myQuery);
            return null;
          * */
        }


        // get the next scheduled event and generate a tile image with details
        private void GenerateImage()
        {

            ObservableCollection<Event> events = fetchNextEvents(true);

            // layout
            //______________________
            // 
            // My session name is...
            //    12:30: Unicorn
            // Come play with my f...
            //    12.30: Cat.
            // Leave Me Alone Wit...
            //    13:00: Main Theater
            // _______________________

            // layout wide: 
            //________________________________________________
            // 
            // My session name is the funniest thing you eve...
            //    12:30: Unicorn
            // Come play with my friends while I make fun of y...
            //    12.30: Cat.
            // Leave Me Alone With My Collection of Pokemon...
            //    13:00: Main Theater
            //___________________________________
            // update tile

            List<bool> options = new List<bool>(2);
            options.Add(true);
            options.Add(false);
            foreach (bool isLarge in options) 
            {
                int fontsize = isLarge ? 22 : 18;
                int tileHeight = getTileHeight(isLarge);
                int tileWidth = getTileWidth(isLarge);
                foreach(bool isBack in options)
                {
                    var source = new BitmapImage();
                    source.CreateOptions = BitmapCreateOptions.None;
                    Uri imageUri;
                    if (isLarge)
                    {  
                        imageUri = new Uri("Images/Icons/wideTileBackground.png", UriKind.Relative);
                    }
                    else
                    {   
                        imageUri = new Uri("Images/Icons/mediumTileBackground.png", UriKind.Relative);
                    }

                    System.Windows.Resources.StreamResourceInfo s = Application.GetResourceStream(imageUri);
                    source.SetSource(s.Stream);
                    var b = new WriteableBitmap(source);

                    var canvas = new Grid();
                    canvas.Width = b.PixelWidth;
                    canvas.Height = b.PixelHeight;

                    var background = new Canvas();
                    background.Height = b.PixelHeight;
                    background.Width = b.PixelWidth;

                    //Created background color as Accent color
                    //SolidColorBrush backColor = new SolidColorBrush((Color)Application.Current.Resources["PhoneAccentColor"]);
                   // background.Background = backColor;

                        // switch on isBack
                         // write up to three sessions on front tile
                    // put description of first on back of tile
                // if < 3 just populate tile, back says "free time! Check out the TableTop Library?"
                // if none then find next within any time frame
                // if none for real then do "no more scheduled events..." // "is PAX over? :( or is it time for the Console Library?!?"
              
                    var textBlock = new TextBlock();
                    textBlock.Inlines.Add(System.Environment.NewLine);
                    if (events == null || events.Count == 0)
                    {
                        if (isBack)
                            textBlock.Inlines.Add("(is it PAX?)");
                        else 
                        {   
                            textBlock.Inlines.Add("nothing");
                            textBlock.Inlines.Add(Environment.NewLine); 
                            textBlock.Inlines.Add(" scheduled...");
                        }
                    }
                    else
                    {
                        if (isBack)
                        {
                            textBlock.TextWrapping = TextWrapping.Wrap;
                            textBlock.Inlines.Add(((Event)events.ElementAt(0)).Details);
                        }
                        else
                        {
                            textBlock.TextWrapping = TextWrapping.NoWrap;
                            for (int count = 0; count < 3; count++)
                            {
                                Event e = (Event)events.ElementAt(count);
                                textBlock.Inlines.Add(e.Name);
                                textBlock.Inlines.Add(System.Environment.NewLine);
                                textBlock.Inlines.Add("   " + e.StartTime + ": " + e.Location);
                            }
                        }
                    }

                    textBlock.FontWeight = FontWeights.Bold;
                    textBlock.TextAlignment = TextAlignment.Left;
                    textBlock.HorizontalAlignment = HorizontalAlignment.Center;
                    textBlock.VerticalAlignment = VerticalAlignment.Center;
                    textBlock.Margin = new Thickness(17);
                    textBlock.Width = b.PixelWidth - textBlock.Margin.Left * 2;
                    textBlock.TextWrapping = TextWrapping.NoWrap;
                    textBlock.Foreground = new SolidColorBrush(Colors.Black); //color of the text on the Tile
                    textBlock.FontSize = fontsize;

                    canvas.Children.Add(textBlock);

                    b.Render(background, null);
                    b.Render(canvas, null);
                    b.Invalidate(); //Draw bitmap

                    using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                    {
                        string filename = isLarge ?  wideTileFilename :  mediumTileFilename;
                        if (isBack)
                        {
                            filename = isLarge ? wideTileBackFilename : mediumTileBackFilename;
                        }
                        using (var st = new IsolatedStorageFileStream(filename, System.IO.FileMode.Create, System.IO.FileAccess.Write, store))
                        {
                            b.SaveJpeg(st, tileWidth, tileHeight, 0, 100);
                        }
                    }
                }
            }
        }

       
    }
}