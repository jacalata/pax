using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PAX7.ViewModel
{
    public class MapViewModel
    {
        private string _imageGroupName;
        public string imageGroupName 
        { 
            get
            {
                return _imageGroupName.ToUpper();
            }
            set
            {
                _imageGroupName = value;
            }
        }
        public List<string> strings { get; set; }
        public List<ImageFile> images { get; set; }
        
        public MapViewModel(string groupName, List<ImageFile> images)
        {
            this.imageGroupName = groupName;
            this.images = images;
            this.strings = new List<string>();
            strings.Add(groupName + "image");
        }
        
        public MapViewModel(string groupName, ImageFile image)
        {
            this.imageGroupName = groupName;
            this.images = new List<ImageFile>();
            images.Add(image);
        }
    }

    public class ImageFile
    {
        public string filename { get; set; }
        public string imageLabel { get; set; }
        public ImageFile(string name, string filename)
        {
            this.filename = filename;
            this.imageLabel = name;
        }

    }

}
