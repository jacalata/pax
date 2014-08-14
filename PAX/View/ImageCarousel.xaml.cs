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
                createImageSetSeattle();
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
        /// Add all boston maps to an imageset and give it a name
        /// </summary>
        private void createImageSetSeattle()
        {
            List<ViewModel.ImageFile> _images = new List<ViewModel.ImageFile>();
            _images.Add(new ViewModel.ImageFile("seattle", "..\\Images\\Seattle\\SeattleMapDowntown.jpg"));
            _imageset = new MapViewModel("Boston", _images);

        }

        /// <summary>
        /// world maps are a kind of birds eye view of all buildings used in the con
        /// there are no world maps for boston because it's entirely contained in the bcec
        /// </summary>
        private void createImageSetWorldMaps()
        {
            //seattle world map files
               List<ViewModel.ImageFile> _images = new List<ViewModel.ImageFile>();
            _images.Add(new ViewModel.ImageFile("world","..\\Images\\Seattle\\map_world.png"));
            _images.Add(new ViewModel.ImageFile("annex","..\\Images\\Seattle\\map_annex.png"));
           _images.Add(new ViewModel.ImageFile("sheraton","..\\Images\\Seattle\\map_sheraton.png"));
           _imageset = new MapViewModel("World Maps", _images);
        }


        private void createImageSetFloorMaps()
        {
            List<ViewModel.ImageFile> _images = new List<ViewModel.ImageFile>();
            /*
            _images.Add(new ViewModel.ImageFile("level 0", "..\\Images\\Boston\\dyamaps\\bcecLevel0.png"));
            _images.Add(new ViewModel.ImageFile("level 1", "..\\Images\\Boston\\dyamaps\\bcecLevel1.png"));
            _images.Add(new ViewModel.ImageFile("level 2", "..\\Images\\Boston\\dyamaps\\bcecLevel2.png"));
            _images.Add(new ViewModel.ImageFile("level 3", "..\\Images\\Boston\\dyamaps\\bcecLevel3.png"));
            _imageset = new MapViewModel("BCEC", _images);
             * */
            _images.Add(new ViewModel.ImageFile("expo 4a", "..\\Images\\Seattle\\map_expo_4_a.PNG"));
            _imageset = new MapViewModel("WSCC", _images);
        }

        private void createImageSetExpoMaps()
        {
            List<ViewModel.ImageFile> _images = new List<ViewModel.ImageFile>();
            /*
            _images.Add(new ViewModel.ImageFile("level 0", "..\\Images\\Boston\\2014official\\Level0.png"));
            _images.Add(new ViewModel.ImageFile("level 1", "..\\Images\\Boston\\2014official\\Level1.png"));
            _images.Add(new ViewModel.ImageFile("level 2", "..\\Images\\Boston\\2014official\\Level2.png"));
            _images.Add(new ViewModel.ImageFile("level 3", "..\\Images\\Boston\\2014official\\Level3.png"));
            _images.Add(new ViewModel.ImageFile("expo hall", "..\\Images\\Boston\\2014official\\expo.png"));
            _images.Add(new ViewModel.ImageFile("indie megabooth", "..\\Images\\Boston\\2014official\\indieMegaBooth.png"));
           // _images.Add(new ViewModel.ImageFile("exhibitor key", "..\\Images\\Boston\\2014official\\exhibitor_directory.png"));
             * */
            _images.Add(new ViewModel.ImageFile("level 0", "..\\Images\\Seattle\\map_level1_2_3.png"));
            _images.Add(new ViewModel.ImageFile("level 1", "..\\Images\\Seattle\\map_level4_6.png"));
            _imageset = new MapViewModel("PAX layout", _images);
        }

        private void createImageSetShuttle()
        {
            List<ViewModel.ImageFile> _images = new List<ViewModel.ImageFile>();
            _images.Add(new ViewModel.ImageFile("shuttle info", "..\\Images\\Boston\\2014official\\shuttleInfo.png"));
            _imageset = new MapViewModel("Shuttles", _images);
        }
          
    }
}
