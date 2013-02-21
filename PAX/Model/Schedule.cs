using System;
using System.Collections.Generic; //IEnumerable, list<string>
using System.Collections.ObjectModel; //observablecollection
using System.IO;
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Windows;
using System.Windows.Resources; //streamresourceinfo
using System.Xml;
using System.Xml.Linq;

using PAX7.Utilicode; //isolated storage settings


namespace PAX7.Model
{
    
    public class Schedule
    {
        public List<string> eventLocations;
        public List<string> eventTypes;
        public List<string> eventDays;

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
                   
        }

        /// <summary>
        /// thrown at the end of GetEvents for the ViewModel
        /// </summary>
        public event EventHandler<ScheduleLoadingEventArgs> ScheduleLoadingComplete;

        /// <summary>
        /// Populate the events collection. 
        /// if we are launching for the first time after an update 
        /// then we need to get it from the new event xml files and also save the data into isolated storage.
        /// then populate the static Events variable from the current data in isolated storage
        /// // seems inefficient but is fast enough so far, can optimize later if necessary.
        /// </summary>
        public void GetEvents()
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains(currentAppVersion))
            {
                ReadXMLEventsIntoStorage(true);
                IsolatedStorageSettings.ApplicationSettings.Add(currentAppVersion, null);
            }
            ObservableCollection<Event> Events = GetSavedEvents();
                
            // throw an event for finished loading, the viewmodel will trigger off it
            if (ScheduleLoadingComplete != null)
            {
                ScheduleLoadingComplete(this,
                  new ScheduleLoadingEventArgs(Events));
            }
        }


        internal void ReadXMLEventsIntoStorage(bool readFromXap)
        {
            ObservableCollection<Event> events = GetXMLEvents(readFromXap);
            SaveEvents(events);
        }

        /// <summary>
        /// takes a list of filenames, loads these files from XML and parses into events, populates the Events var
        /// </summary>
        /// <param name="readFromXap">if first run, read from xap, else from IsolatedStorage</param>
        /// <param name="filenames">list of files to read events from: if null, read the included contents.xml file to find it</param>
        internal ObservableCollection<Event> GetXMLEvents(bool readFromXap, List<String> filenames = null)
        {
            ObservableCollection<Event> events = new ObservableCollection<Event>();

            if (filenames == null)
            {
                filenames = GetFilenames(readFromXap);
            }
            // the schema file contains the days and locations and kinds of events
            // maybe more efficient to just read these from the events list themselves? 
            // removes worries about being in sync!
            // GetEventCategories(readFromXap); 

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

                    // now build the category lists - days, rooms and kinds
                    var readEventLocations = (from item in eventItems
                                              where item.Kind != "Social"
                                              select item.Location).Distinct();
                    foreach (var loc in readEventLocations)
                    {
                        string strippedLoc = stripCommonWords(loc.ToString());
                        if (!eventLocations.Contains(strippedLoc))
                            eventLocations.Add(strippedLoc);
                    }

                    var readEventTypes = (from item in eventItems
                                          select item.Kind).Distinct();
                    foreach (var type in readEventTypes)
                    {
                        if (!eventTypes.Contains(type))
                            eventTypes.Add(type);
                    }
                    var readEventDays = (from item in eventItems
                                         select item.StartTime).Distinct();
                    foreach (var datetime in readEventDays)
                    {
                        var day = datetime.DayOfWeek.ToString();
                        if (!eventDays.Contains(day))
                            eventDays.Add(day);
                    }
                    RecordScheduleCreationDate(readFromXap);

                }
                catch (Exception e)
                {
                    // catch any exceptions to prevent a single bad file from crashing the app.
                    // also log it so I know it happened.
                    LittleWatson.ReportException(e, "exception reading schedule from " + filename);
                }
            }

            return events;

        }


        /// <summary>
        /// retrieve all events from isolated storage
        /// also populates global variables locations, days and types
        /// </summary>
        internal ObservableCollection<Event> GetSavedEvents()
        {
            ObservableCollection<Event> events = new ObservableCollection<Event>();
            foreach (Object o in IsolatedStorageSettings.ApplicationSettings.Values)
            {
                if (o is Event)
                    events.Add((Event)o);
            }

            // get config data
            IsolatedStorageSettings.ApplicationSettings.TryGetValue("eventLocations", out this.eventLocations);
            IsolatedStorageSettings.ApplicationSettings.TryGetValue("eventDays", out this.eventDays);
            IsolatedStorageSettings.ApplicationSettings.TryGetValue("eventTypes", out this.eventTypes);

            return events;
        }

        /// <summary>
        /// Will be called the first time we have loaded new events from xml files, 
        /// saves all the events back to IsolatedStorage. Checks the existing events in Storage
        /// to avoid losing any stars, but clears all of them before saving the new ones.
        /// </summary>
        internal void SaveEvents(ObservableCollection<Event> events)
        {
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;
            // in case we are updating over the same set of events, copy over any that have been starred
            foreach (Event e in events)
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

            // clear and then save config data
            settings.Remove("eventLocations");
            settings.Remove("eventTypes");
            settings.Remove("eventDays");
            settings.Add("eventLocations", eventLocations);
            settings.Add("eventTypes", eventTypes);
            settings.Add("eventDays", eventDays);

            settings.Save();
        }


        #region event handling for downloading updated schedules


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
            string xmlFileName = "latestVersionInfo.xml"+DateTime.Now.Ticks;
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
                    IsolatedStorageFileStream isolatedStorageFileStream = store.CreateFile(xmlFileName);
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
                        string date = versionDate.Attribute("lastupdated").Value;
                        // null ref exception, serverLastUpdate got default date. hmm. 
                        serverLastUpdate = safeParse(date, "none", xmlFileName);
                        fileDownloadLocation = versionDate.Attribute("fileLocation").Value;
                    }
                    isolatedStorageFileStream.Close();
                }
                catch(Exception exception)
                {
                    LittleWatson.ReportException(exception, "reading version info");
                    hasUpdateAvailable = false;
                }

                if ((false == IsolatedStorageSettings.ApplicationSettings.TryGetValue(IsoStoreSettings.IsoStoreLastUpdatedRecord, out localLastUpdate)) 
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

            }
            // trigger an event so the viewmodel knows we've looked it up
            if (evt_updateCheckComplete != null)
            {
                evt_updateCheckComplete(this, evtArgs);
            }
            // save our value to the settings
            IsoStoreSettings.SaveToSettings<bool>(IsoStoreSettings.IsoStoreHasUpdateAvailable, hasUpdateAvailable);
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

                if (!weAreTesting)  MessageBox.Show("Schedule Updated!"); // should move this out to the view and trigger an event for it
                IsoStoreSettings.SaveToSettings<DateTime>(IsoStoreSettings.IsoStoreLastUpdatedRecord, DateTime.Now);

            }
            catch (Exception ex)
            {
                if (!weAreTesting) MessageBox.Show("Something went wrong updating your schedule data. Try again later?");
                LittleWatson.ReportException(ex, "attempted at " + DateTime.Now.ToString());
            }
            IsoStoreSettings.SaveToSettings<bool>(IsoStoreSettings.IsoStoreHasUpdateAvailable, false);

        }

        #endregion


        #region helper methods: internal to be exposed for testing


        /// <summary>
        /// helper method to clean a string before inserting it into our url
        /// </summary>
        /// <param name="initialString">string we got from somewhere</param>
        /// <returns>string that has been cleaned for a url</returns>
        internal string makeURLSafe(string initialString)
        {
            string urlString = initialString.Replace(" ", ""); //no spaces in our urls
            if (!urlString.Equals("")) urlString += @"/";
            return urlString;
        }


        /// <summary>
        /// Test method - clear the isolated storage of all settings and events
        /// </summary>
        internal void NukeAllStorage()
        {
            IsolatedStorageSettings.ApplicationSettings.Clear();
        }

        /// <summary>
        /// Remove 'tourney' 'theatre' or 'room' from a location. 
        /// </summary>
        /// <param name="input">The location string as read from an xml file</param>
        /// <returns>The modified location string</returns>
        internal string stripCommonWords(string input)
        {
            List<string> cutWords = new List<String>();
            cutWords.Add("Tourney");
            cutWords.Add("Room");
            cutWords.Add("Theater");
            cutWords.Add("Theatre");
            string output = input;
            foreach (string word in cutWords)
            {
                if (input.Contains(word))
                    output = output.Remove(input.IndexOf(word) - 1, word.Length + 1);
            }
            return output;
        }

        /// <summary>
        /// Record from contents.xml the date that this schedule was created, so we know whether the server copy is more recent
        /// </summary>
        /// <param name="readFromXap"></param>
        /// <param name="filename"></param>
        internal void RecordScheduleCreationDate(bool readFromXap, string filename = null)
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
                    IsoStoreSettings.SaveToSettings<DateTime>(IsoStoreSettings.IsoStoreLastUpdatedRecord, updateDate);
                }
                else
                {
                    LittleWatson.ReportException(new Exception(),
                        "contents.xml file with no last updated date - " + (readFromXap ? "from xap" : "from update"));
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
        internal DateTime safeParse(string dateString, string eventName = null, string filename = null)
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