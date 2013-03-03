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
    public partial class TelerikImageViewer : PhoneApplicationPage
    {
        public TelerikImageViewer()
        {
            InitializeComponent();
            Loaded += PageLoaded;
        }

        public MapViewModel _imageset {get; set;}
        public string mytestfile { get; set; }
        public List<string> _strings { get; set; }
        public void PageLoaded(object sender, RoutedEventArgs e)
        {
            List<ViewModel.ImageFile> _images = new List<ViewModel.ImageFile>();
            _images.Add(new ViewModel.ImageFile("boston", "..\\Images\\Boston\\boston.png"));
            _images.Add(new ViewModel.ImageFile("bcc", "..\\Images\\Boston\\bostonconventioncenter.png"));
            _images.Add(new ViewModel.ImageFile("city", "..\\Images\\Boston\\bostonwaterfront.png"));
            _imageset = new MapViewModel("my group", _images);
            _strings = new List<string>();
            _strings.Add("me!");
            _strings.Add("you!");
            slideView.ItemsSource = _imageset.images;
            PageHeader.DataContext = _imageset;
        }
    }
}
