using System;
using System.Net;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Resources; //streamresourceinfo
using System.Collections.ObjectModel; //observablecollection
using System.IO.IsolatedStorage;
using System.Linq;
using System.Xml;
using System.Xml.Linq;
using System.IO;
using System.Collections.Generic; //IEnumerable, list<string>


namespace PAX7.Model
{
    
    public class Schedule
    {
        private bool hasUpdateAvailable = false;
        private string currentAppVersion = "update3.1";
        public List<string> eventLocations;
        public List<string> eventTypes;
        public List<string> eventDays;
        public IEnumerable<Event> Events { get; set; }
        public DateTime dtHardcodedScheduleUpdateTime;

        private string uriScheduleZip = "http://paxwp7.nfshost.com/East/2012/schedule.zip";
        private string uriVersionInfo = "http://paxwp7.nfshost.com/latestversion.txt";

        //global vars for web download
        IsolatedStorageFileStream isolatedStorageFileStream;

        // adding a constructor to explicitly init. the Events collection and the days/types/locations
        public Schedule()
        {
            ObservableCollection<Event> events = new ObservableCollection<Event>();
            Events = events;
            eventDays = new List<string>();
            eventLocations = new List<string>();
            eventTypes = new List<string>();
            hasUpdateAvailable = false;
            dtHardcodedScheduleUpdateTime = new DateTime(2012, 3, 31); // HARDCODED the day I updated the schedule 
            //before submitting to the app store
        }


        // contains update logic to check if we are launching for the first time after an update 
        // if so we need to get the new event xml files and load the data into isolated storage.
        // if no update, just load the current event data from isolated storage
        // this is probably a little inefficient, for the case where we are doing an update. refactor it. 
        public void GetEvents()
        {            
            if ( !IsolatedStorageSettings.ApplicationSettings.Contains(currentAppVersion) )
            {
                GetXMLEvents();
                SaveEvents();
                IsolatedStorageSettings.ApplicationSettings.Add(currentAppVersion, null);


                if (IsolatedStorageSettings.ApplicationSettings.Contains("lastUpdated"))
                {
                    IsolatedStorageSettings.ApplicationSettings.Remove("lastUpdated");
                }
                IsolatedStorageSettings.ApplicationSettings.Add("lastUpdated", dtHardcodedScheduleUpdateTime);
            }
            GetSavedEvents();
    
            // throw an event for finished loading, the viewmodel will trigger off it
            if (ScheduleLoadingComplete != null)
            {
                ScheduleLoadingComplete(this,
                  new ScheduleLoadingEventArgs(Events));
            }
        }

        /// <summary>
        /// kick off a web request for the version info on the server, to see if there is new schedule data available
        /// </summary>
        public void checkForNewSchedule()
        {
            WebClient webClient = new WebClient();
            webClient.OpenReadCompleted += new OpenReadCompletedEventHandler(webClient_VersionInfoCompleted);
            webClient.OpenReadAsync(new Uri(uriVersionInfo));
        }

        /// <summary>
        /// refers to the updateAvailable flag to see if a new schedule was found on our last check
        /// </summary>
        /// <returns>bool true if a new schedule is available</returns>
        public bool HasUpdateAvailable()
        {
            bool isAvailable = false;
            bool hasChecked = IsolatedStorageSettings.ApplicationSettings.TryGetValue("hasUpdateAvailable", out isAvailable);
            return isAvailable;
        }

        public EventHandler evt_updateCheckComplete;
        public class FileInfo
        {
            public string filename;
            public FileInfo(string filename)
            {
                this.filename = filename;
            }
            public FileInfo() { }
        };

        /// <summary>
        /// completed downloading latest version info, now we can check the values and set the updateAvailable flag
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void webClient_VersionInfoCompleted(object sender, OpenReadCompletedEventArgs evtArgs)
        {
            // save to isostore for posterity records? would it b better to save as appsettings values?
            string xmlFileName = "latestVersionInfo.xml"+DateTime.Now;
            IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
            if (evtArgs.Result != null) //we gots a file!
            {
                try
                {
                    #region Isolated Storage Copy Code
                    isolatedStorageFileStream = new IsolatedStorageFileStream(xmlFileName, FileMode.OpenOrCreate, store);
                    long fileLength = (long)evtArgs.Result.Length;
                    byte[] byteImage = new byte[fileLength];
                    evtArgs.Result.Read(byteImage, 0, byteImage.Length);
                    isolatedStorageFileStream.Write(byteImage, 0, byteImage.Length);
                    isolatedStorageFileStream.Close();
                    #endregion
                }
                catch (Exception exception)
                {
                    LittleWatson.ReportException(exception, "downloading version info");
                }

                // read back from isostore. 
                XDocument versionDoc = new XDocument();
                DateTime serverLastUpdate = new DateTime();
                DateTime localLastUpdate;
                try
                {
                    isolatedStorageFileStream = new IsolatedStorageFileStream(xmlFileName, FileMode.Open, store);
                    versionDoc = XDocument.Load(isolatedStorageFileStream);
                    var versionDate = versionDoc.Descendants("version").FirstOrDefault();
                    if (versionDate != null)
                        serverLastUpdate = safeParse(versionDate.Attribute("lastupdated").Value);
                    isolatedStorageFileStream.Close();
                }
                catch(Exception exception)
                {
                    LittleWatson.ReportException(exception, "reading version info");
                }

                if (false == IsolatedStorageSettings.ApplicationSettings.TryGetValue("lastUpdated", out localLastUpdate))
                {
                    //we have no recorded local update time, assume the server is newer
                    hasUpdateAvailable = true;
                }
                else if (serverLastUpdate.CompareTo(localLastUpdate) > 0)
                {
                    // server time is later than local time
                    hasUpdateAvailable = true;
                }
                else
                {
                    // we found a local update time and it's more recent or equally recent than the server
                    hasUpdateAvailable = false;
                }

                // save our value to the settings
                if( IsolatedStorageSettings.ApplicationSettings.Contains("hasUpdateAvailable") == true )
                {
                    // key already exists, remove it  
                    IsolatedStorageSettings.ApplicationSettings.Remove("hasUpdateAvailable");
                }
                IsolatedStorageSettings.ApplicationSettings.Add("hasUpdateAvailable", hasUpdateAvailable);
                IsolatedStorageSettings.ApplicationSettings.Save();
                // trigger an event so the viewmodel knows we've got a new value

                if (evt_updateCheckComplete != null)
                {
                    evt_updateCheckComplete(this, evtArgs);
                }
            }
        }


        /// <summary>
        /// get refreshed schedule files from the website
        /// this code taken wholesale from http://www.dotnetcurry.com/ShowArticle.aspx?ID=586
        /// </summary>
        public void DownloadNewEventFiles()
        {
            WebClient webClient = new WebClient();
            //  webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(webClient_DownloadProgressChanged);
            webClient.OpenReadCompleted += new OpenReadCompletedEventHandler(webClient_OpenReadCompleted);
            webClient.OpenReadAsync(new Uri(uriScheduleZip), "contents.xml");
            // check how much iso space we have left
            // might need to request extra isolated storage space to store the file? 
        }


        public EventHandler evt_downloadScheduleComplete;
        /// <summary>
        /// completed downloading and opening new event files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void webClient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            List<string> filenames = new List<string>();
            try
            {
                if (e.Result != null)
                {
                    IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
                    #region Isolated Storage Copy Code
                    /* for debugging: save the original schedule.zip to isostore.
                    isolatedStorageFileStream = new IsolatedStorageFileStream("scheudle.zip", FileMode.OpenOrCreate, store);
                    long fileLength2 = (long)e.Result.Length;
                    byte[] byteImage2 = new byte[fileLength2];
                    e.Result.Read(byteImage2, 0, byteImage2.Length);
                    isolatedStorageFileStream.Write(byteImage2, 0, byteImage2.Length);
                    isolatedStorageFileStream.Close();
                    */
                    #endregion
                    #region extract files from zip
                    Uri part = new Uri(Convert.ToString(e.UserState), UriKind.Relative); 
                    // Read manifest from zip file 
                    StreamResourceInfo zipRI = new StreamResourceInfo(e.Result, null);
                    StreamResourceInfo manifestInfo = App.GetResourceStream(zipRI, part);
                    StreamReader reader = new StreamReader(manifestInfo.Stream); 
                    XDocument document = XDocument.Load(reader);

                    var blah = document.Descendants(); 
                    string me = blah.ToString();
                    var files = from item in document.Descendants("file") // so's this
                                select new FileInfo
                                {
                                    filename = item.Attribute("name").Value
                                };
                    foreach (var file in files)
                    {
                        string filename = file.filename;
                        filenames.Add(filename);
                        StreamResourceInfo fileInfo = Application.GetResourceStream(zipRI, (new Uri(filename, UriKind.Relative)));
                        StreamReader fileReader = new StreamReader(fileInfo.Stream);
                        isolatedStorageFileStream = new IsolatedStorageFileStream(filename, FileMode.OpenOrCreate, store);
                        string fileRead = fileReader.ReadToEnd();
                        long fileLength = fileRead.Length;
                        byte[] byteImage = new byte[fileLength];
                        isolatedStorageFileStream.Write(byteImage, 0, byteImage.Length);
                        isolatedStorageFileStream.Close();
                        fileReader.Close();
                    }
                    #endregion
                }
                GetXMLEvents(filenames, false);
                SaveEvents();

                if (IsolatedStorageSettings.ApplicationSettings.Contains("lastUpdated"))
                {
                    IsolatedStorageSettings.ApplicationSettings.Remove("lastUpdated");
                }
                IsolatedStorageSettings.ApplicationSettings.Add("lastUpdated", DateTime.Now);

                MessageBox.Show("Schedule Updated!");

            }
            catch (Exception ex)
            {
                MessageBox.Show("Something went wrong updating your schedule data. Try again later?");              
            }


        }

        // overload that calls GetXMLEvents with the default file set
        public void GetXMLEvents()
        {
            List<String> filenames = new List<string>();

            //load events from XML
            //friday, saturday, sunday, Tabletop, D&D, contests, social
            filenames.Add("XML\\Friday.xml");
            filenames.Add("XML\\Saturday.xml");
            filenames.Add("XML\\Sunday.xml");
            filenames.Add("XML\\TableTop.xml");
            filenames.Add("XML\\DnD.xml");
            filenames.Add("XML\\contests.xml");
            filenames.Add("XML\\social.xml");
            GetXMLEvents(filenames, true); //isFirstRun = true;
        }

        // takes a list of filenames, loads these files from XML and parses into events, populates the Events var
        public void GetXMLEvents(List<String> filenames, bool isFirstRun)
        {
            ObservableCollection<Event> events = new ObservableCollection<Event>();

            // the schema file contains the days and locations and kinds of events
            readSchemaFile(); 

            foreach (string filename in filenames)
            {
                try
                {
                     XDocument dataDoc;
                    // for the first run only, we are getting the xml from the xap resources, not isolated storage
                    if (isFirstRun)
                    {
                        dataDoc = XDocument.Load(filename);
                    }
                    else
                    {
                        IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
                        Stream filestream = new IsolatedStorageFileStream(filename, System.IO.FileMode.Open, store);
                        dataDoc = XDocument.Load(filestream);
                    }
           
                    var eventItems = from item in dataDoc.Descendants("Event")
                                     let stamp = safeParse(item.Attribute("datetime").Value)
                                     orderby stamp ascending
                                     select new Event
                                     {
                                         Kind = item.Attribute("kind").Value,
                                         Name = item.Attribute("name").Value,
                                         Details = item.Attribute("description").Value,
                                         EndTime = safeParse(item.Attribute("end").Value),
                                         StartTime = stamp,
                                         Location = item.Attribute("location").Value
                                     };
                    foreach (Event eventItem in eventItems)
                    {
                        events.Add(eventItem);
                    }

                }
                catch (XmlException e)
                {
                    // catch this exception to prevent a single bad file from crashing the app.
                    // also log it so I know it happened.
                    LittleWatson.ReportException(e, filename);
                }
            }

            Events = events;
            
        }

        /// <summary>
        /// extract my date parsing method. All dates are expected to be in USA format, and if one fails, I will insert a 
        /// random date so we can still add the rest of the event details
        /// </summary>
        /// <param name="dateString">string representing a date</param>
        /// <returns>the parsed Datetime object</returns>
        private DateTime safeParse(string dateString)
        {
            DateTime parsedDate = new DateTime();
            try
            {
                parsedDate = DateTime.Parse(dateString, new System.Globalization.CultureInfo("en-US"));
            }
            catch (FormatException e)
            {
                parsedDate = DateTime.Parse("1/1/2000 09:15:00 PM"); // make up a date I guess?
                LittleWatson.ReportException(e, dateString);
            }
            return parsedDate;
        }

        /// <summary>
        /// instead of hardcoding the location and days of the events, read them from a schema file
        /// haven't yet implemented that, so pulling the hardcoded bits together into here for now
        /// </summary>
        private void readSchemaFile()
        {
            // read from xml - not yet implemented

            IsolatedStorageSettings.ApplicationSettings.Remove("eventLocations");
            IsolatedStorageSettings.ApplicationSettings.Remove("eventTypes");
            IsolatedStorageSettings.ApplicationSettings.Remove("eventDays");

            eventLocations.Add("Main");
            eventLocations.Add("Arachnid");
            eventLocations.Add("Cat");
            eventLocations.Add("Manticore");
            eventLocations.Add("Merman");
            eventLocations.Add("Naga");
            eventLocations.Add("Wyvern");
            eventLocations.Add("Jamspace");
            eventLocations.Add("Tabletop");

            eventTypes.Add("Panel");
            eventTypes.Add("Omegathon");
            eventTypes.Add("Contest");
            eventTypes.Add("Show");
            eventTypes.Add("Social");

            eventDays.Add("Friday");
            eventDays.Add("Saturday");
            eventDays.Add("Sunday");

            // save to isolated storage
            IsolatedStorageSettings.ApplicationSettings.Add("eventLocations", eventLocations);
            IsolatedStorageSettings.ApplicationSettings.Add("eventTypes", eventTypes);
            IsolatedStorageSettings.ApplicationSettings.Add("eventDays", eventDays);
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        public void GetSavedEvents()
        {
            ObservableCollection<Event> events = new ObservableCollection<Event>();
            foreach (Object o in IsolatedStorageSettings.ApplicationSettings.Values)
            {
                if (o is Event)
                    events.Add((Event)o);
            }
            Events = events;

            // if these calls fail the app is pretty messed up, I should fail and cry or something
            IsolatedStorageSettings.ApplicationSettings.TryGetValue("eventLocations", out eventLocations);
            IsolatedStorageSettings.ApplicationSettings.TryGetValue("eventDays", out eventDays);
            IsolatedStorageSettings.ApplicationSettings.TryGetValue("eventTypes", out eventTypes);

        }

        /// <summary>
        /// Test method - clear the isolated storage of all settings and events
        /// </summary>
        public void NukeAllStorage()
        {
            IsolatedStorageSettings.ApplicationSettings.Clear();
        }


        /// <summary>
        /// Will be called the first time we have loaded new events from xml files, 
        /// saves all the events back to IsolatedStorage. Checks the existing events in Storage
        /// to avoid losing any stars, but clears all of them before saving the new ones.
        /// </summary>
        public void SaveEvents()
        {
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            // in case we are updating over the same set of events, copy over any that have been starred
            foreach (Event e in Events)
            {
                if (settings.Contains(e.Name))
                {
                    Event original = new Event();
                    settings.TryGetValue<Event>(e.Name, out original);
                    if (original.Star)
                        e.Star = original.Star;
                    settings.Remove(original.Name);
                 }
                 settings.Add(e.Name, e.GetCopy());
            }
            settings.Save();
        }

        public void Clear()
        {
            Events = null;
        }

        public event EventHandler<ScheduleLoadingEventArgs> ScheduleLoadingComplete;


    } // end Schedule class
    
    public class ScheduleLoadingEventArgs : EventArgs
    {
        public IEnumerable<Event> Results { get; private set; }

        public ScheduleLoadingEventArgs(IEnumerable<Event> results)
        {
            Results = results;
        }
    }

}