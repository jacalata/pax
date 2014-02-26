using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;
using Microsoft.Phone.Controls;
using PAX7.ViewModel;
using PAX7.Utilicode; // iso store

namespace PAX7.View
{
    public partial class ImageCarousel : PhoneApplicationPage
    {


        /// <summary>
        /// has to be public for datacontext
        /// </summary>
        public MapViewModel _imageset { get; set; }

        /// <summary>
        /// int, fetched from iso store on construction
        /// </summary>
        private int conventionID = -1;

        /// <summary>
        /// constructor - add page load event handler where we do the work
        /// </summary>
        public ImageCarousel()
        {
            InitializeComponent();
            conventionID = IsoStoreSettings.GetDefaultConvention();
            Loaded += PageLoaded;

        }

        /// <summary>
        /// We choose which maps to display based on the query string in the NavigatedTo url
        /// how does this interact with the loaded event handler?
        /// </summary>
        /// <param name="e"></param>
        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            string newparameter = this.NavigationContext.QueryString["Image"];
            Dictionary<String, String> attributes = new Dictionary<string, string>();
            attributes.Add("conventionID", conventionID.ToString());
            attributes.Add("imageView", newparameter);
            ((App)Application.Current).appSession.tagEvent("ViewMaps", attributes);
            if (newparameter.Equals("map_world"))
            {
                createImageSetWorldMaps();
            }
            else if (newparameter.Equals("map_expo"))
            {
                createImageSetExpoMaps();
            }
            else if (newparameter.Equals("map_level"))
            {
                createImageSetFloorMaps();
            }
            else //city map
            {
                createImageSetCityMaps();
            }
        }

        /// <summary>
        /// set the data context of the page to the current image set as chosen in OnNavigatedTo
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public void PageLoaded(object sender, RoutedEventArgs e)
        {
            ApplicationTitle.Text = _imageset.imageGroupName;
            slideView.ItemsSource = _imageset.images;
        }


        /// <summary>
        /// Add all city maps to an imageset and give it a name
        /// </summary>
        private void createImageSetCityMaps()
        {
            List<ViewModel.ImageFile> _images = new List<ViewModel.ImageFile>();
            if (conventionID == (int)IsoStoreSettings.Convention.PAXEAST)
            {
                _images.Add(new ViewModel.ImageFile("boston", "..\\Images\\Boston\\boston.png"));
                _images.Add(new ViewModel.ImageFile("BCEC parking", "..\\Images\\Boston\\bcecParking.png"));
                _images.Add(new ViewModel.ImageFile("nearby parking", "..\\Images\\Boston\\bcecSurroundingParking.png"));
                _images.Add(new ViewModel.ImageFile("local streets", "..\\Images\\Boston\\bcecSurroundingStreets.png"));
                _images.Add(new ViewModel.ImageFile("MBTA Subway map", "..\\Images\\Boston\\BostonMBTAMap.jpg"));
                _imageset = new MapViewModel("Boston", _images);
            }

            else if (conventionID == (int)IsoStoreSettings.Convention.PAXAUS)
            {
                _images.Add(new ViewModel.ImageFile("showgrounds", "..\\Images\\Melbourne\\showgroundsSurrounds.png"));
                _images.Add(new ViewModel.ImageFile("downtown", "..\\Images\\Melbourne\\InnerMelbourne.png"));
                _images.Add(new ViewModel.ImageFile("overview", "..\\Images\\Melbourne\\cityToShowgrounds.png"));
                _images.Add(new ViewModel.ImageFile("local train map", "..\\Images\\Melbourne\\trains.png"));
                _imageset = new MapViewModel("Melbourne", _images);
            }
            else // if (conventionID == (int)App.Convention.PAXPRIME) // might as well show prime as a default
            {
                if (conventionID != (int)IsoStoreSettings.Convention.PAXPRIME)
                {
                    LittleWatson.ReportException(new Exception(), "convention ID did not match Aus/East/Prime: was recorded as " + conventionID);
                }
                _images.Add(new ViewModel.ImageFile("birdseye", "..\\Images\\Seattle\\SeattleMapDowntown.jpg"));
                _imageset = new MapViewModel("City", _images);

            }

        }

        /// <summary>
        /// world maps are a kind of birds eye view of all buildings used in the con
        /// there are no world maps for boston because it's entirely contained in the bcec
        /// </summary>
        private void createImageSetWorldMaps()
        {
            List<ViewModel.ImageFile> _images = new List<ViewModel.ImageFile>();

            if (conventionID == (int)IsoStoreSettings.Convention.PAXAUS)
            {
                _images.Add(new ViewModel.ImageFile("pax", "..\\Images\\Melbourne\\2013official\\WorldMap.png"));
                _images.Add(new ViewModel.ImageFile("showgrounds", "..\\Images\\Melbourne\\Showgrounds.png"));
            }
            else if (conventionID == (int)IsoStoreSettings.Convention.PAXEAST)
            {
                // boston doesn't have a world map. Hmm..
                LittleWatson.ReportException(new Exception(), "For some reason we looked for world maps for Boston. Shouldn't be needing them...");
                return;
            }
            else // if (conventionID == (int)App.Convention.PAXPRIME) // might as well show prime as a default
            {
                if (conventionID != (int)IsoStoreSettings.Convention.PAXPRIME)
                {
                    LittleWatson.ReportException(new Exception(), "convention ID did not match Aus/East/Prime: was recorded as " + conventionID);
                }
                _images.Add(new ViewModel.ImageFile("world", "..\\Images\\Seattle\\world_map_2013.png"));
                _images.Add(new ViewModel.ImageFile("annex", "..\\Images\\Seattle\\annex_map_2013.png"));
                _images.Add(new ViewModel.ImageFile("sheraton+hyatt", "..\\Images\\Seattle\\area_map_2013.png"));
            }
            _imageset = new MapViewModel("World Maps", _images);
        
        }

        /// <summary>
        /// floor layout within the building for east and prime
        /// </summary>
        private void createImageSetFloorMaps()
        {
            List<ViewModel.ImageFile> _images = new List<ViewModel.ImageFile>();
            if (conventionID == (int)IsoStoreSettings.Convention.PAXAUS)
            {
                // aus doesn't have a floor map. .
                LittleWatson.ReportException(new Exception(), "For some reason we looked for floor maps for Aus. Shouldn't be needing them...");
                return;
            }
            else if (conventionID == (int)IsoStoreSettings.Convention.PAXEAST)
            {
                _images.Add(new ViewModel.ImageFile("level 0", "..\\Images\\Boston\\floorplanOverlays\\bcecLevel0.png"));
                _images.Add(new ViewModel.ImageFile("level 1", "..\\Images\\Boston\\floorplanOverlays\\bcecLevel1.png"));
                _images.Add(new ViewModel.ImageFile("level 2", "..\\Images\\Boston\\floorplanOverlays\\bcecLevel2.png"));
                _images.Add(new ViewModel.ImageFile("level 3", "..\\Images\\Boston\\floorplanOverlays\\bcecLevel3.png"));
                _imageset = new MapViewModel("BCEC", _images);
            } 
            else // if (conventionID == (int)App.Convention.PAXPRIME) // might as well show prime as a default
            {
                if (conventionID != (int)IsoStoreSettings.Convention.PAXPRIME)
                {
                    LittleWatson.ReportException(new Exception(), "convention ID did not match Aus/East/Prime: was recorded as " + conventionID);
                }

                _images.Add(new ViewModel.ImageFile("level 2,3", "..\\Images\\Seattle\\2013_level2_level3.png"));
                _images.Add(new ViewModel.ImageFile("4,6", "..\\Images\\Seattle\\2013_lvl4_lvl6.png"));
                _images.Add(new ViewModel.ImageFile("annex", "..\\Images\\Seattle\\annex_map_2013.png"));
                _images.Add(new ViewModel.ImageFile("map legend", "..\\Images\\Seattle\\2013_legend.png"));
                _imageset = new MapViewModel("Convention Center", _images);
            }

        }

        /// <summary>
        /// expo hall layout and exhibitor name list
        /// </summary>
        private void createImageSetExpoMaps()
        {
            List<ViewModel.ImageFile> _images = new List<ViewModel.ImageFile>();
            if (conventionID == (int)IsoStoreSettings.Convention.PAXAUS)
            {
                _images.Add(new ViewModel.ImageFile("expo", "..\\Images\\Melbourne\\2013official\\Expo_Hall.png"));
                _images.Add(new ViewModel.ImageFile("tabletop", "..\\Images\\Melbourne\\2013official\\MAPS_1.png"));
                _images.Add(new ViewModel.ImageFile("theatres", "..\\Images\\Melbourne\\2013official\\MAPS_2.jpg"));

            }
            else if (conventionID == (int)IsoStoreSettings.Convention.PAXEAST)
             {
                 _images.Add(new ViewModel.ImageFile("level 0", "..\\Images\\Boston\\2013official\\Level0.png"));
                 _images.Add(new ViewModel.ImageFile("level 1", "..\\Images\\Boston\\2013official\\Level1.png"));
                 _images.Add(new ViewModel.ImageFile("level 2", "..\\Images\\Boston\\2013official\\Level2.png"));
                 _images.Add(new ViewModel.ImageFile("level 3", "..\\Images\\Boston\\2013official\\Level3.png"));
                 _images.Add(new ViewModel.ImageFile("exhibitors", "..\\Images\\Boston\\2013official\\exhibitor_hall.png"));
                 _images.Add(new ViewModel.ImageFile("exhibitor key", "..\\Images\\Boston\\2013official\\exhibitor_directory.png"));
            }
              else // if (conventionID == (int)App.Convention.PAXPRIME) // might as well show prime as a default
             {
                 if (conventionID != (int)IsoStoreSettings.Convention.PAXPRIME)
                 {
                     LittleWatson.ReportException(new Exception(), "convention ID did not match Aus/East/Prime: was recorded as " + conventionID);
                 }
                 _images.Add(new ViewModel.ImageFile("level 4a", "..\\Images\\Seattle\\expo_2013_Lvl4_part1.png"));
                 _images.Add(new ViewModel.ImageFile("level 4b", "..\\Images\\Seattle\\expo_2013_level4pt2.png"));
                 _images.Add(new ViewModel.ImageFile("level 6", "..\\Images\\Seattle\\expo_2013_lvl6.png"));
             }
            _imageset = new MapViewModel("Expo Hall", _images);
        }
    }
}
