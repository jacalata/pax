using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;
using Microsoft.Phone.Controls;
using Microsoft.Phone.Shell;

namespace PAX7.View
{
    public class ImageSet
    {
        public string imageGroupName { get; set; }
        public List<ImageFile> images { get; set; }

        public ImageSet(string groupName, List<ImageFile> images)
        {
            this.imageGroupName = groupName;
            this.images = images;
        }
    }

    public class ImageFile
    {
        public string filename;
        public string imageLabel;
        public ImageFile(string name, string filename)
        {
            this.filename = filename;
            this.imageLabel = name;
        }

    }

    public partial class Page1 : PhoneApplicationPage
    {
        public ImageSet _imageSet;
        public Page1()
        {
            InitializeComponent();
            Loaded += MainPage_Loaded;
        }
        void MainPage_Loaded(object sender, RoutedEventArgs e)
        {

            ImageFile image = new ImageFile("boston", "/Images/Boston/boston.png");
            List<ImageFile> images = new List<ImageFile>();
            images.Add(image);
            _imageSet = new ImageSet("group 1", images);
            ContentPanel.DataContext = _imageSet;
            PageContent.DataContext = _imageSet;
        }
    }
}