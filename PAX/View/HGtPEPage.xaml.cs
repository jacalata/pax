using System;
using System.Net;
using System.Windows; //Application
using System.Windows.Controls;
using Microsoft.Phone.Controls;
using System.IO; //Stream
using System.Windows.Resources; // StreamResourceInfo
using System.IO.IsolatedStorage;
using System.Collections.Generic; //List


namespace PAX7.View
{
    public partial class HGtPEPage : PhoneApplicationPage
    {
       
        public HGtPEPage()
        {
            InitializeComponent(); 
            browser.Loaded += WebBrowser_OnLoaded;
        }

        private void WebBrowser_OnLoaded(object sender, RoutedEventArgs e)
        {
            SaveFilesToIsoStore();
            browser.Navigate(new Uri("index.html", UriKind.Relative));
        }

        protected override void OnNavigatedFrom(System.Windows.Navigation.NavigationEventArgs e)
        {            
            base.OnNavigatedFrom(e);
        }

        protected override void OnNavigatedTo(System.Windows.Navigation.NavigationEventArgs e)
        {
            base.OnNavigatedTo(e);
        }

        private void SaveFilesToIsoStore()
        {
            //These files must match what is included in the application package,
            //or BinaryStream.Dispose below will throw an exception.
            string[] files = {"index.html", "2am.html"
        };

            IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();

            if (false == isoStore.FileExists(files[0]))
            {
                foreach (string f in files)
                {
                    StreamResourceInfo sr = Application.GetResourceStream(new Uri(f, UriKind.Relative));
                    using (BinaryReader br = new BinaryReader(sr.Stream))
                    {
                        byte[] data = br.ReadBytes((int)sr.Stream.Length);
                        SaveToIsoStore(f, data);
                    }
                }
            }
        }

        private void SaveToIsoStore(string fileName, byte[] data)
        {
            string strBaseDir = string.Empty;
            string delimStr = "/";
            char[] delimiter = delimStr.ToCharArray();
            string[] dirsPath = fileName.Split(delimiter);

            //Get the IsoStore
            IsolatedStorageFile isoStore = IsolatedStorageFile.GetUserStoreForApplication();

            //Recreate the directory structure
            for (int i = 0; i < dirsPath.Length - 1; i++)
            {
                strBaseDir = System.IO.Path.Combine(strBaseDir, dirsPath[i]);
                isoStore.CreateDirectory(strBaseDir);
            }

            //Remove existing file
            if (isoStore.FileExists(fileName))
            {
                isoStore.DeleteFile(fileName);
            }

            //Write the file
            using (BinaryWriter bw = new BinaryWriter(isoStore.CreateFile(fileName)))
            {
                bw.Write(data);
                bw.Close();
            }
        }



        //from Shawn Wildermuth's blog
        private void backButton_Click(object sender, EventArgs e)
        {
            try
            {
                browser.InvokeScript("eval", "history.go(-1)");
            }
            catch
            {
                // Eat error
            }
        }

        private void refreshButton_Click(object sender, EventArgs e)
        {
            try
            {
                browser.InvokeScript("eval", "history.go()");
            }
            catch
            {
                // Eat error
            }
        }

        private void forwardButton_Click(object sender, EventArgs e)
        {
            try
            {
                browser.InvokeScript("eval", "history.go(1)");
            }
            catch
            {
                // Eat error
            }
        }


    }
}