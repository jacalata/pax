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
using Google.WebAnalytics;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Logging;
using PAX7.Utilicode;
using PAX7.ViewModel;

namespace PAX7.View
{
    public partial class ImageCarousel : PhoneApplicationPage
    {


        /// <summary>
        /// has to be public for datacontext
        /// </summary>
        public MapViewModel _imageset { get; set; }

        /// <summary>
        /// constructor - add page load event handler where we do the work
        /// </summary>
        public ImageCarousel()
        {
            InitializeComponent();
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
            else if (newparameter.Equals("shuttle")) //boston only
            {
                createImageSetShuttle();
            }
            else //city map
            {
                createImageSetCity();
            }

            AnalyticsTracker tracker = new AnalyticsTracker();
            tracker.Track("ImageCarousel", "Loaded", newparameter);
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

        #region citymaps
        private void createImageSetCity()
        {
            createImageSetBoston();
            //createImageSetSeattle
            //createImageSetSouth
            //createImageSetAus
        }

        /// <summary>
        /// Add all boston maps to an imageset and give it a name
        /// </summary>
        private void createImageSetBoston()
        {
            List<ViewModel.ImageFile> _images = new List<ViewModel.ImageFile>();
            _images.Add(new ViewModel.ImageFile("boston", "..\\Images\\Boston\\boston.jpg"));
            _images.Add(new ViewModel.ImageFile("BCEC parking", "..\\Images\\Boston\\bcecParking.jpg"));
            _images.Add(new ViewModel.ImageFile("nearby parking", "..\\Images\\Boston\\bcecSurroundingParking.jpg"));
            _images.Add(new ViewModel.ImageFile("local streets", "..\\Images\\Boston\\bcecSurroundingStreets.jpg"));
            _images.Add(new ViewModel.ImageFile("MBTA Subway map", "..\\Images\\Boston\\BostonMBTAMap.jpg"));
            _imageset = new MapViewModel("Boston", _images);
        }

        /// <summary>
        /// Add all seattle maps to an imageset and give it a name
        /// </summary>
        private void createImageSetSeattle()
        {
            List<ViewModel.ImageFile> _images = new List<ViewModel.ImageFile>();
            _images.Add(new ViewModel.ImageFile("seattle", "..\\Images\\Seattle\\SeattleMapDowntown.jpg"));
            _imageset = new MapViewModel("Seattle", _images);
        }

        private void createImageSetSouth()
        {
            List<ViewModel.ImageFile> _images = new List<ViewModel.ImageFile>();
            _images.Add(new ViewModel.ImageFile("SACC surrounds", "..\\Images\\South\\googlemap.jpg"));
            _imageset = new MapViewModel("San Antonio", _images);
        }

        private void createImageSetAus()
        {
            List<ViewModel.ImageFile> _images = new List<ViewModel.ImageFile>();
            _images.Add(new ViewModel.ImageFile("melbourne", "..\\Images\\Melbourne\\downtown.jpg"));
            _imageset = new MapViewModel("Melbourne", _images);
            
        }
        #endregion


        /// <summary>
        /// world maps are a kind of birds eye view of all buildings used in the con
        /// there are no world maps for boston or south because it's entirely contained in the convention center
        /// </summary>
        private void createImageSetWorldMaps()
        {
               List<ViewModel.ImageFile> _images = new List<ViewModel.ImageFile>();
            /*
            _images.Add(new ViewModel.ImageFile("world","..\\Images\\Seattle\\map_world.png"));
            _images.Add(new ViewModel.ImageFile("annex","..\\Images\\Seattle\\map_annex.png"));
           _images.Add(new ViewModel.ImageFile("sheraton","..\\Images\\Seattle\\map_sheraton.png"));
             * */

               _images.Add(new ViewModel.ImageFile("world", "..\\Images\\Melbourne\\world.jpg"));
           _imageset = new MapViewModel("World Maps", _images);
        }


        private void createImageSetFloorMaps()
        {
            List<ViewModel.ImageFile> _images = new List<ViewModel.ImageFile>();
            
            _images.Add(new ViewModel.ImageFile("level 0", "..\\Images\\Boston\\dyamaps2015\\bcecLevel0.png"));
            _images.Add(new ViewModel.ImageFile("level 1", "..\\Images\\Boston\\dyamaps2015\\bcecLevel1.png"));
            _images.Add(new ViewModel.ImageFile("level 2", "..\\Images\\Boston\\dyamaps2015\\bcecLevel2.png"));
            _images.Add(new ViewModel.ImageFile("level 3", "..\\Images\\Boston\\dyamaps2015\\bcecLevel3.png"));
            _imageset = new MapViewModel("BCEC", _images);             
            
           // _images.Add(new ViewModel.ImageFile("expo 4a", "..\\Images\\Seattle\\map_expo_4_a.PNG"));
            
             /*AUS
            _images.Add(new ViewModel.ImageFile("level 0-1", "..\\Images\\Melbourne\\level0-1.jpg"));
            _images.Add(new ViewModel.ImageFile("level 0-2", "..\\Images\\Melbourne\\level0-2.jpg"));
            _images.Add(new ViewModel.ImageFile("promenade", "..\\Images\\Melbourne\\promenade.jpg"));
            */

            /*SOUTH
             _images.Add(new ViewModel.ImageFile("level 1", "..\\Images\\South\\sacc_1.jpg"));
             _images.Add(new ViewModel.ImageFile("level 2", "..\\Images\\South\\sacc_2.jpg"));
            _imageset = new MapViewModel("SACC", _images);
             */
        }

        private void createImageSetExpoMaps()
        {
            List<ViewModel.ImageFile> _images = new List<ViewModel.ImageFile>();
            
            _images.Add(new ViewModel.ImageFile("map 1", "..\\Images\\Boston\\2015official\\bcec_left.png"));
            _images.Add(new ViewModel.ImageFile("map 2", "..\\Images\\Boston\\2015official\\bcec_right.png"));
            _images.Add(new ViewModel.ImageFile("lines", "..\\Images\\Boston\\2015official\\bcec_lines.jpg"));
            _images.Add(new ViewModel.ImageFile("expo hall", "..\\Images\\Boston\\2015official\\expo.jpg"));
             

            /*
            _images.Add(new ViewModel.ImageFile("level 0", "..\\Images\\Seattle\\map_level1_2_3.png"));
            _images.Add(new ViewModel.ImageFile("level 1", "..\\Images\\Seattle\\map_level4_6.png"));
             * */
            
           //  _images.Add(new ViewModel.ImageFile("expo", "..\\Images\\Melbourne\\expo.jpg"));
          //  _images.Add(new ViewModel.ImageFile("expo", "..\\Images\\South\\exhibit_hall.jpg"));

            _imageset = new MapViewModel("Expo Hall", _images);
        }

        private void createImageSetShuttle()
        {
            List<ViewModel.ImageFile> _images = new List<ViewModel.ImageFile>();
            _images.Add(new ViewModel.ImageFile("shuttle info", "..\\Images\\Boston\\2015official\\shuttle_info.JPG"));
            _images.Add(new ViewModel.ImageFile("routes 1-6", "..\\Images\\Boston\\2015official\\shuttles_1_6.JPG"));
            _images.Add(new ViewModel.ImageFile("route 7", "..\\Images\\Boston\\2015official\\shuttles_7.JPG"));
            _imageset = new MapViewModel("Shuttles", _images);
        }
          
    }
}
