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
        public List<string> eventLocations;
        public List<string> eventTypes;
        public List<string> eventDays;
        public IEnumerable<Event> Events { get; set; }

        internal string IsoStoreLastUpdatedRecord; // the name for the value we look at to check the last schedule update date
        internal string IsoStoreHasUpdateAvailable;// the name for the value we look at to check if a new update is available
        internal bool hasUpdateAvailable = false;

        internal string currentAppVersion = "update3.1";
        internal string uriScheduleZip = @"http://paxwp7.nfshost.com/East/2012/schedule.zip"; //gets updated from the version file
        internal string uriVersionInfo = @"http://paxwp7.nfshost.com/latestversion.txt";

        //global vars for web download
        private IsolatedStorageFileStream isolatedStorageFileStream;
        private bool weAreTesting; //avoid trying to do a message box in automated testing

        // adding a constructor to explicitly init. the Events collection and the days/types/locations
        public Schedule(bool runUnderTest = false)
        {
            ObservableCollection<Event> events = new ObservableCollection<Event>();
            Events = events;
            eventDays = new List<string>();
            eventLocations = new List<string>();
            eventTypes = new List<string>();
            hasUpdateAvailable = false;
            weAreTesting = runUnderTest;

            string conventionName = "";
            if (IsolatedStorageSettings.ApplicationSettings.Contains("CurrentConvention") == true)
            {
                IsolatedStorageSettings.ApplicationSettings.TryGetValue("CurrentConvention", out conventionName);
            }
            string urlConventionName = makeURLSafe(conventionName);
            uriScheduleZip = @"http://paxwp7.nfshost.com/" + urlConventionName + @"schedule.zip";
            uriVersionInfo = @"http://paxwp7.nfshost.com/" + urlConventionName + @"latestversion.txt";
                
            // add convention name so I can track these separately for each
            IsoStoreLastUpdatedRecord = urlConventionName + "lastUpdated";
            IsoStoreHasUpdateAvailable = urlConventionName + "hasUpdateAvailable";
            
        }

        /// <summary>
        /// helper method to clean a string before inserting it into our url
        /// </summary>
        /// <param name="initialString">string we got from somewhere</param>
        /// <returns>string that has been cleaned for a url</returns>
        private string makeURLSafe(string initialString)
        {
            string urlString = initialString.Replace(" ", ""); //no spaces in our urls
            if (!urlString.Equals("")) urlString += @"/";
            return urlString;
        }


        /// <summary>
        /// thrown at the end of GetEvents for the ViewModel
        /// </summary>
        public event EventHandler<ScheduleLoadingEventArgs> ScheduleLoadingComplete;

        /// <summary>
        /// Populate the events collection. 
        /// if we are launching for the first time after an update 
        ///  then we need to get it from the new event xml files and also save the data into isolated storage.
        /// else if no update, just load the current data in isolated storage
        /// </summary>
        public void GetEvents()
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains(currentAppVersion))
            {
                GetXMLEvents(true);
                SaveEvents();
                IsolatedStorageSettings.ApplicationSettings.Add(currentAppVersion, null);
            }
            else
            {
                GetSavedEvents();
            }    
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
        /// <param name="uri">optional: url to download the new schedule data from (for tests)</param>
        public void checkForNewSchedule(string uri = null)
        {
            if (uri == null) uri = uriVersionInfo;
            //hacky: append unique parameter to avoid webclient caching. Ok for now as we only check on demand?
            uri = AppendUniqueParameter(uri);
            WebClient webClient = new WebClient();
            webClient.OpenReadCompleted += new OpenReadCompletedEventHandler(webClient_VersionInfoCompleted);
            webClient.OpenReadAsync(new Uri(uri));
        }

        /// <summary>
        /// refers to the updateAvailable flag to see if a new schedule was found on our last check
        /// </summary>
        /// <returns>bool true if a new schedule is available</returns>
        public bool HasUpdateAvailable()
        {
            bool isAvailable = false;
            bool hasChecked = IsolatedStorageSettings.ApplicationSettings.TryGetValue(IsoStoreHasUpdateAvailable, out isAvailable);
            return isAvailable;
        }

        /// <summary>
        /// event triggered during webClient_VersionInfoCompleted, waited on by the ViewModel
        /// </summary>
        public EventHandler evt_updateCheckComplete;

        /// <summary>
        /// completed downloading latest version info, now we can check the values and set the updateAvailable flag
        /// this method is triggered by checkForNewSchedule
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="evtArgs"></param>
        void webClient_VersionInfoCompleted(object sender, OpenReadCompletedEventArgs evtArgs)
        {
            // save to isostore for posterity records? would it b better to save as appsettings values?
            string xmlFileName = "latestVersionInfo.xml"+DateTime.Now;
            bool downloaded = false;
            IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
            try
            {
                if ((evtArgs.Result) != null)
                    downloaded = true;
            }
            catch (Exception e)
            { 
                // any error downloading the file: eg no connection, 404, server 5xx
                LittleWatson.ReportException(e, "downloading version info");
                hasUpdateAvailable = false;
            }

            if (downloaded) //we gots a file!
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
                    LittleWatson.ReportException(exception, "copying downloaded version info to isostore");
                }

                // read back from isostore. 
                XDocument versionDoc = new XDocument();
                DateTime serverLastUpdate = new DateTime();
                DateTime localLastUpdate;
                string fileDownloadLocation = null;
                try
                {
                    isolatedStorageFileStream = new IsolatedStorageFileStream(xmlFileName, FileMode.Open, store);
                    versionDoc = XDocument.Load(isolatedStorageFileStream);
                    var versionDate = versionDoc.Descendants("version").FirstOrDefault();
                    if (versionDate != null)
                    {
                        serverLastUpdate = safeParse(versionDate.Attribute("lastUpdated").Value, "none", "xmlFileName");
                        fileDownloadLocation = versionDate.Attribute("fileLocation").Value;
                    }
                    isolatedStorageFileStream.Close();
                }
                catch(Exception exception)
                {
                    LittleWatson.ReportException(exception, "reading version info");
                    hasUpdateAvailable = false;
                }

                if ( (false == IsolatedStorageSettings.ApplicationSettings.TryGetValue(IsoStoreLastUpdatedRecord, out localLastUpdate)) 
                     || (serverLastUpdate.CompareTo(localLastUpdate) > 0) )
                {
                    //we have no recorded local update time, assume the server is newer
                    //or, server time is later than local time, server is def. newer
                    hasUpdateAvailable = true;
                    this.uriScheduleZip = fileDownloadLocation;
                }
                else
                {
                    // we found a local update time and it's more recent or equally as old as the server
                    hasUpdateAvailable = false;
                }

                // save our value to the settings
                if( IsolatedStorageSettings.ApplicationSettings.Contains(IsoStoreHasUpdateAvailable) == true )
                {
                    // key already exists, remove it  
                    IsolatedStorageSettings.ApplicationSettings.Remove(IsoStoreHasUpdateAvailable);
                }
                IsolatedStorageSettings.ApplicationSettings.Add(IsoStoreHasUpdateAvailable, hasUpdateAvailable);
                IsolatedStorageSettings.ApplicationSettings.Save();
            }
            // trigger an event so the viewmodel knows we've looked it up
            if (evt_updateCheckComplete != null)
            {
                evt_updateCheckComplete(this, evtArgs);
            }
        }

        /// <summary>
        /// get refreshed schedule files from the website
        /// this code taken wholesale from http://www.dotnetcurry.com/ShowArticle.aspx?ID=586
        /// </summary>
        /// <param name="uri">optional: url to download the new schedule data from (for tests)</param>
        public void DownloadNewEventFiles(string uri = null)
        {
            if (uri == null) uri = uriScheduleZip;
            uri = AppendUniqueParameter(uri);
            WebClient webClient = new WebClient();
            //  webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(webClient_DownloadProgressChanged);
            webClient.OpenReadCompleted += new OpenReadCompletedEventHandler(webClient_OpenReadCompleted);
            webClient.OpenReadAsync(new Uri(uri), "contents.xml");
            // check how much iso space we have left
            // might need to request extra isolated storage space to store the file? 
        }

        /// <summary>
        /// event triggered by webClient_OpenReadCompleted for the viewmodel
        /// </summary>
        public EventHandler evt_downloadScheduleComplete;

        /// <summary>
        /// completed downloading and opening new event files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void webClient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            List<string> filenames = new List<string>();
            try
            {
                if (e.Result != null)
                {
                    IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
                    #region Isolated Storage Copy Code
                    /* for debugging: save the original schedule.zip to isostore.
                    isolatedStorageFileStream = new IsolatedStorageFileStream("schedule.zip", FileMode.OpenOrCreate, store);
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
                    var files = from item in document.Descendants("file")
                                select new NamedItem
                                {
                                    name = item.Attribute("name").Value
                                };
                    foreach (var file in files)
                    {
                        string filename = file.name;
                        filenames.Add(filename);
                        StreamResourceInfo fileInfo = Application.GetResourceStream(zipRI, (new Uri(filename, UriKind.Relative)));
                        StreamReader fileReader = new StreamReader(fileInfo.Stream);
                        string fileRead = fileReader.ReadToEnd();
                        StreamWriter fileWriter = new StreamWriter(new IsolatedStorageFileStream(filename, FileMode.OpenOrCreate, store));
                        fileWriter.Write(fileRead);
                        fileWriter.Close();
                        fileReader.Close();
                    }
                    #endregion
                }
                GetXMLEvents(false, filenames);
                SaveEvents();

                if (!weAreTesting)  MessageBox.Show("Schedule Updated!");

            }
            catch (Exception ex)
            {
                if (!weAreTesting) MessageBox.Show("Something went wrong updating your schedule data. Try again later?");
                LittleWatson.ReportException(ex, "attempted at " + DateTime.Now.ToString());
            }
            //actually we don't have any viewmodel reaction to this yet. Plausible I'll want it in the future?
            // if (evt_downloadScheduleComplete != null)
            // {
            //     evt_downloadScheduleComplete(this, new EventArgs());
            // }

        }

        /// <summary>
        /// takes a list of filenames, loads these files from XML and parses into events, populates the Events var
        /// </summary>
        /// <param name="readFromXap">if first run, read from xap, else from IsolatedStorage</param>
        /// <param name="filenames">list of files to read events from: if null, read the included contents.xml file to find it</param>
        public void GetXMLEvents( bool readFromXap, List<String> filenames = null)
        {
            ObservableCollection<Event> events = new ObservableCollection<Event>();

            if (filenames == null)
            {
                filenames = GetFilenames(readFromXap);
            }
            // the schema file contains the days and locations and kinds of events
            GetEventCategories(readFromXap); 

            foreach (string filename in filenames)
            {
                try
                {
                     XDocument dataDoc;
                    // for the first run only, we are getting the xml from the xap resources, not isolated storage
                    if (readFromXap)
                    {
                        try
                        {
                            dataDoc = XDocument.Load(filename);
                        }
                        catch (XmlException e)
                        {
                            //this will catch actual malformed xml, not schema specific errors
                            LittleWatson.ReportException(e, "malformed xml in file " + filename);
                            //don't try and read the datadoc we didn't create, just continue to the next file
                            continue;
                        }
                    }
                    else
                    {
                        IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
                        Stream filestream = new IsolatedStorageFileStream(filename, System.IO.FileMode.Open, store);
                        dataDoc = XDocument.Load(filestream);
                    }
           
                    var eventItems = from item in dataDoc.Descendants("Event")
                                     let stamp = safeParse(item.Attribute("datetime").Value, item.Attribute("name").Value, filename)
                                     orderby stamp ascending
                                     select new Event
                                     {
                                         Kind = item.Attribute("kind").Value,
                                         Name = item.Attribute("name").Value,
                                         Details = item.Attribute("description").Value,
                                         EndTime = safeParse(item.Attribute("end").Value, item.Attribute("name").Value, filename),
                                         StartTime = stamp,
                                         Location = item.Attribute("location").Value
                                     };
                    foreach (Event eventItem in eventItems)
                    {
                        events.Add(eventItem);
                    }
                    RecordScheduleCreationDate(readFromXap);

                }
                catch (Exception e)
                {
                    // catch any exceptions to prevent a single bad file from crashing the app.
                    // also log it so I know it happened.
                    LittleWatson.ReportException(e, "exception reading schedule from "+filename);
                }
            }

            Events = events;
            
        }
        
        #region helper methods: internal to be exposed for testing

        /// <summary>
        /// Test method - clear the isolated storage of all settings and events
        /// </summary>
        internal void NukeAllStorage()
        {
            IsolatedStorageSettings.ApplicationSettings.Clear();
        }

        /// <summary>
        /// Record from contents.xml the date that this schedule was created, so we know whether the server copy is more recent
        /// </summary>
        /// <param name="readFromXap"></param>
        /// <param name="filename"></param>
        internal void RecordScheduleCreationDate(bool readFromXap, string filename=null)
        {
            if (filename == null)
            {
                filename = "XML\\contents.xml";
            }
             try
             {
                 XDocument dataDoc;
                 if (readFromXap)
                 {
                     dataDoc = XDocument.Load(filename);
                 }
                 else
                 {
                     IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
                     Stream filestream = new IsolatedStorageFileStream(filename, System.IO.FileMode.Open, store);
                     dataDoc = XDocument.Load(filestream);
                 }
                 // read the date that this schedule was created and make a note of it in isostore
                 var updateDetails = from item in dataDoc.Descendants("update")
                                  select new NamedItem
                                  {
                                      name = item.Attribute("date").Value
                                  };

                 if (updateDetails.Count<NamedItem>() > 0)
                 {
                     DateTime updateDate = DateTime.Parse(updateDetails.First<NamedItem>().name);

                     if (IsolatedStorageSettings.ApplicationSettings.Contains(IsoStoreLastUpdatedRecord))
                     {
                         IsolatedStorageSettings.ApplicationSettings.Remove(IsoStoreLastUpdatedRecord);
                     }
                     IsolatedStorageSettings.ApplicationSettings.Add(IsoStoreLastUpdatedRecord, updateDate);
                 }
                 else
                 {
                     LittleWatson.ReportException(new Exception(), 
                         "contents.xml file with no last updated date - " + (readFromXap? "from xap": "from update"));
                 }
             }
             catch (Exception e)
             {
                 // catch any exceptions to prevent a single bad file from crashing the app.
                 // also log it so I know it happened.
                 // this will catch actual malformed xml, but not schema specific errors
                 // TODO: what should I actually do if we couldn't read the contents? pass back a default list and hope that works? 
                 // currently it will fail out here
                 LittleWatson.ReportException(e, "(possible bad xml) exception reading from " + filename);
             }
        }

        /// <summary>
        /// Hacky function to make uri unique each time, so that we don't cache the update check in webclient
        /// http://stackoverflow.com/questions/9859137/should-i-disable-webclient-caching?rq=1
        /// </summary>
        /// <param name="uri"></param>
        /// <returns></returns>
        internal string AppendUniqueParameter(string uri)
        {
            uri += "?MakeRequestUnique=";
            uri += Environment.TickCount;
            return uri;
        }


        /// <summary>
        ///  read contents.xml to find out which files to read events from
        /// </summary>
        /// <param name="readFromXap">bool: if this is an update or first run,
        /// then we are getting the xml from the xap resources, not isolated storage</param>
        /// <param name="filename">Optional: for test purposes, allow passing in an arbitrary file</param>
        /// <returns>a list of filenames</returns>
        internal List<String> GetFilenames(bool readFromXap, string filename = null)
        {
            List<String> filenames = new List<string>();
            if (filename == null)
            {
                filename = "XML\\contents.xml";
            }
            try
            {
                XDocument dataDoc;
                if (readFromXap)
                {
                    dataDoc = XDocument.Load(filename);
                }
                else
                {
                    IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
                    Stream filestream = new IsolatedStorageFileStream(filename, System.IO.FileMode.Open, store);
                    dataDoc = XDocument.Load(filestream);
                }

                var files = from item in dataDoc.Descendants("file")
                            select new NamedItem
                            {
                                name = item.Attribute("name").Value
                            };
                foreach (var file in files)
                {
                    string name = file.name;
                    filenames.Add("XML\\" + name);
                }

              
            }
            catch (Exception e)
            {
                // catch any exceptions to prevent a single bad file from crashing the app.
                // also log it so I know it happened.
                // this will catch actual malformed xml, but not schema specific errors
                // TODO: what should I actually do if we couldn't read the contents? pass back a default list and hope that works?
                LittleWatson.ReportException(e, "(possible bad xml) exception reading from " + filename);
            }
            return filenames;
        }


        /// <summary>
        /// instead of hardcoding the location and days of the events, read them from a schema file
        /// populates the data sets in isolated storage
        /// </summary>
        /// <param name="readFromXap">for the first run only, read from xap resources, else read from isolated storage</param>
        /// <param name="filename">for test purposes: allow passing in an arbitrary file</param>
        /// <returns>list of the days of the event: only used in testing, should remove if I can verify the IsoStore straight up</returns>
        internal List<String> GetEventCategories(bool readFromXap, string filename = null)
        {
            if (filename == null)
            {
                filename = "XML\\ConventionData.xml";
            }
            try
            {
                XDocument dataDoc;
                // for the first run only, we are getting the xml from the xap resources, not isolated storage
                if (readFromXap)
                {
                    dataDoc = XDocument.Load(filename);
                }
                else
                {
                    IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
                    Stream filestream = new IsolatedStorageFileStream(filename, System.IO.FileMode.Open, store);
                    dataDoc = XDocument.Load(filestream);
                }

                var locations = from item in dataDoc.Descendants("location")
                            select new NamedItem 
                            {
                                name = item.Attribute("name").Value
                            };
                foreach (var loc in locations)
                {
                    eventLocations.Add(loc.name);
                }

                var kinds = from item in dataDoc.Descendants("kind")
                            select new NamedItem
                            {
                                name = item.Attribute("name").Value
                            };
                foreach (var kind in kinds)
                {
                    eventTypes.Add(kind.name);
                }


                var days = from item in dataDoc.Descendants("day")
                            select new NamedItem
                            {
                                name = item.Attribute("name").Value
                            };
                foreach (var day in days)
                {
                    eventDays.Add(day.name);
                }
            }
            catch (Exception e)
            {
                // catch any exceptions to prevent a single bad file from crashing the app.
                // also log it so I know it happened.
                // this will catch actual malformed xml, but not schema specific errors
                LittleWatson.ReportException(e, "(possible bad xml) exception reading days/rooms/types from " + filename);
            }

            // clear and then save to isolated storage
            IsolatedStorageSettings.ApplicationSettings.Remove("eventLocations");
            IsolatedStorageSettings.ApplicationSettings.Remove("eventTypes");
            IsolatedStorageSettings.ApplicationSettings.Remove("eventDays");
            IsolatedStorageSettings.ApplicationSettings.Add("eventLocations", eventLocations);
            IsolatedStorageSettings.ApplicationSettings.Add("eventTypes", eventTypes);
            IsolatedStorageSettings.ApplicationSettings.Add("eventDays", eventDays);
            IsolatedStorageSettings.ApplicationSettings.Save();
            return eventDays; //for testing
        }


        /// <summary>
        /// wrapper class for retrieving strings from xml docs using linq
        /// </summary>
        private class NamedItem
        {
            public string name;
            public NamedItem(string name)
            {
                this.name = name;
            }
            public NamedItem() { }
        };

        /// <summary>
        /// extract my date parsing method. All dates are expected to be in USA format, and if one fails, I will insert a 
        /// random date so we can still add the rest of the event details
        /// </summary>
        /// <param name="dateString">string representing a date</param>
        /// <returns>the parsed Datetime object</returns>
        internal DateTime safeParse(string dateString, string eventName, string filename)
        {
            DateTime parsedDate = new DateTime();
            try
            {
                parsedDate = DateTime.Parse(dateString, new System.Globalization.CultureInfo("en-US"));
            }
            catch (FormatException e)
            {
                parsedDate = DateTime.Parse("1/1/2000 09:15:00 PM"); // make up a date I guess?
                LittleWatson.ReportException(e, "invalid dateString in event " + eventName + " in file " + filename);
            }
            return parsedDate;
        }

#endregion

        /// <summary>
        /// retrieve all events from isolated storage
        /// </summary>
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