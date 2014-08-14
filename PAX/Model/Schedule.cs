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

        internal string currentAppVersion = "update4.0";
        internal string uriScheduleZip = "";
        internal string uriVersionInfo = "";

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
        /// if we are launching for the first time after an updated xap
        /// then we need to get it from the new event xml files and also save the data into isolated storage.
        /// then populate the static Events variable from the current data in isolated storage
        /// // seems inefficient but is fast enough so far, can optimize later if necessary.
        /// </summary>
        public void GetEvents()
        {
            if (!IsolatedStorageSettings.ApplicationSettings.Contains(currentAppVersion))
            {
                ObservableCollection<Event> events = GetXMLEvents(true);
                if (events != null) // could be null if the events were older than our locally updated schedule, or xml parsing failed. 
                {
                    SaveEvents(events);
                }
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


        /// <summary>
        /// takes a list of filenames, loads these files from XML and parses into events, populates the Events var
        /// </summary>
        /// <param name="readFromXap">if first run, read from xap, else from IsolatedStorage</param>
        /// <param name="filenames">list of files to read events from: if null, read the included contents.xml file to find it</param>
        internal ObservableCollection<Event> GetXMLEvents(bool readFromXap, List<String> filenames = null)
        {
            ObservableCollection<Event> events = new ObservableCollection<Event>();

            string contentsFile = readFromXap ? "XML\\contents.xml" : "contents.xml";
            // check if these files are a newer schedule version than what we have
            XDocument versionXDoc = GetXDocFromFilename(readFromXap, contentsFile);
            if (filenames == null)
            {
                filenames = ReadFilenamesFromXdoc(readFromXap, versionXDoc);
            }
            ScheduleVersionData scheduleData = new ScheduleVersionData(contentsFile);
            parseXDocToScheduleVersionData(versionXDoc, scheduleData);
            if (scheduleData.versionNumber > IsoStoreSettings.GetScheduleVersion())
            {
                foreach (string filename in filenames)
                {
                    XDocument dataDoc = GetXDocFromFilename(readFromXap, filename);
                    ParseXDocToEvents(dataDoc, events, filename);
                }
            }
            else
            {
                events = null;
            }
            SaveScheduleVersionDataToIsoStore(scheduleData);
            return events;
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
        public event EventHandler evt_updateCheckComplete;

        /// <summary>
        /// completed downloading latest version info, now we can check the values and set the updateAvailable flag
        /// this method is triggered by checkForNewSchedule
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="evtArgs"></param>
        void webClient_VersionInfoCompleted(object sender, OpenReadCompletedEventArgs evtArgs)
        {
            //clear any old failure marker
            IsoStoreSettings.SaveToSettings<bool>(IsoStoreSettings.IsoStoreUpdateCheckFailed, false);
            // save to isostore for posterity records? would it b better to save as appsettings values?
            string xmlFileName = "latestVersionInfo.txt"+DateTime.Now.Ticks;
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
                // cutting the little watson because it is usually not going to be a reason I care about 
                //LittleWatson.ReportException(e, "downloading version info");
                // if the check was made from the ui, the method that catches our 'finished checking' event will 
                // look at this value and let the user know.
                IsoStoreSettings.SaveToSettings<bool>(IsoStoreSettings.IsoStoreUpdateCheckFailed, true);
                downloaded = false;
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
                catch (Exception exception) //file exceptions
                {
                    LittleWatson.ReportException(exception, "copying downloaded version info to isostore");
                    IsoStoreSettings.SaveToSettings<bool>(IsoStoreSettings.IsoStoreUpdateCheckFailed, true);
                }

                // read back from isostore. 
                XDocument versionDoc = new XDocument();
                ScheduleVersionData scheduleData = new ScheduleVersionData(xmlFileName);
                try
                {
                    isolatedStorageFileStream = new IsolatedStorageFileStream(xmlFileName, FileMode.Open, store);
                    versionDoc = XDocument.Load(isolatedStorageFileStream);
                    parseXDocToScheduleVersionData(versionDoc, scheduleData);
                }
                catch(Exception exception) //file IO exceptions
                {
                    LittleWatson.ReportException(exception, "reading version info file");
                    IsoStoreSettings.SaveToSettings<bool>(IsoStoreSettings.IsoStoreUpdateCheckFailed, true);
                    hasUpdateAvailable = false;
                }

                if (scheduleData.versionNumber > IsoStoreSettings.GetScheduleVersion())
                {
                    // server is newer
                    hasUpdateAvailable = true;
                    this.uriScheduleZip = scheduleData.fileDownloadLocation;
                }
                else 
                {
                    // we found a local version and it's more recent or equally as old as the server
                    hasUpdateAvailable = false;
                }

            }
            // save our value to the settings
            IsoStoreSettings.SaveToSettings<bool>(IsoStoreSettings.IsoStoreHasUpdateAvailable, hasUpdateAvailable);
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
        /// event triggered by webClient_OpenReadCompleted for the test framework
        /// </summary>
        public EventHandler<ScheduleDownloadEventArgs> evt_downloadScheduleComplete;


        /// <summary>
        /// completed downloading and opening new event files
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void webClient_OpenReadCompleted(object sender, OpenReadCompletedEventArgs e)
        {
            List<string> filenames = new List<string>();
            bool updateSucceeded = false;
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
                    StreamResourceInfo contentsInfo = App.GetResourceStream(zipRI, part);
                    StreamReader contentsReader = new StreamReader(contentsInfo.Stream);
                    XDocument contentsDocument = XDocument.Load(contentsReader);
                    List<string> files = ReadFilenamesFromXdoc(false, contentsDocument);

                    ObservableCollection<Event> events = new ObservableCollection<Event>();
                    foreach (var file in files)
                    {
                        // save each file to storage, then we'll call a method to parse the files into events
                        filenames.Add(file);
                        StreamResourceInfo fileInfo = Application.GetResourceStream(zipRI, (new Uri(file, UriKind.Relative)));
                        StreamReader fileReader = new StreamReader(fileInfo.Stream);
                        XDocument xdoc = XDocument.Load(fileReader);
                        ParseXDocToEvents(xdoc, events, file);
                            /*save file to isolated storage for possible debugging*/
                        string fileRead = fileReader.ReadToEnd();
                        StreamWriter fileWriter = new StreamWriter(new IsolatedStorageFileStream(file, FileMode.OpenOrCreate, store));
                        fileWriter.Write(fileRead);
                        fileWriter.Close();
                             /* */
                        fileReader.Close();
                    }
                    SaveEvents(events);
                    ScheduleVersionData scheduleData = new ScheduleVersionData(part.ToString());
                    parseXDocToScheduleVersionData(contentsDocument, scheduleData);
                    SaveScheduleVersionDataToIsoStore(scheduleData);
                    #endregion
                }
                updateSucceeded = true;
                IsoStoreSettings.SaveToSettings<DateTime>(IsoStoreSettings.IsoStoreLastUpdatedRecord, DateTime.Now);

            }
            catch (Exception ex)
            {
                updateSucceeded = false;
                LittleWatson.ReportException(ex, "attempted at " + DateTime.Now.ToString());
            }
            IsoStoreSettings.SaveToSettings<bool>(IsoStoreSettings.IsoStoreHasUpdateAvailable, false);

            // trigger an event to show the user
            if (evt_downloadScheduleComplete != null)
            {
                var args = new ScheduleDownloadEventArgs(updateSucceeded);
                evt_downloadScheduleComplete(this, args);
            }
        }

        #endregion

        #region saving objects to isostore 

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
        /// but it doesn't actually delete them...
        /// </summary>
        internal void SaveEvents(ObservableCollection<Event> events)
        {
            IsolatedStorageSettings settings = IsolatedStorageSettings.ApplicationSettings;

            // read the current set of events in storage. Store the name of any starred items
            // then delete all the events
            List<String> starredEvents = new List<string>();
            List<Event> oldEvents = new List<Event>();
            foreach (Object o in IsolatedStorageSettings.ApplicationSettings.Values)
                {
                    if (o is Event)
                    {
                        if (((Event)o).Star == true)
                            starredEvents.Add(((Event)o).Name);
                        oldEvents.Add((Event)o);
                    }
                }
            //inefficient but safe
            foreach (Event e in oldEvents)
            {
                settings.Remove(e.Name); //collection may not be modified while enumerating over it
            }
            oldEvents.Clear();


            // add each new event to storage. Set the Starred property for any that are in the list created above.
            foreach (Event e in events)
            {
                if (starredEvents.Contains(e.Name))
                {
                    e.Star = true;
                }
                while (settings.Contains(e.Name))
                {
                    e.Name = e.Name + "*";
                }
                settings.Add(e.Name, e.GetCopy());
            }

            // clear and then save config data with the new days/rooms
            settings.Remove("eventLocations");
            settings.Remove("eventTypes");
            settings.Remove("eventDays");
            settings.Add("eventLocations", eventLocations);
            settings.Add("eventTypes", eventTypes);
            settings.Add("eventDays", eventDays);

            settings.Save();
        }


        /// <summary>
        /// given a populated scheduleData object, save the relevant details to IsoStore
        /// </summary>
        /// <param name="scheduleData"></param>
        internal void SaveScheduleVersionDataToIsoStore(ScheduleVersionData scheduleData)
        {
            IsoStoreSettings.SaveToSettings<DateTime>(IsoStoreSettings.IsoStoreScheduleCreationDate, scheduleData.creationDate);
            IsoStoreSettings.SaveToSettings<int>(IsoStoreSettings.IsoStoreScheduleVersionNumber, scheduleData.versionNumber);
        }

        #endregion

        #region Xdoc->Object methods

        /// <summary>
        /// given a populated xdocument, pull out the events and populate the collection that was passed in
        /// </summary>
        /// <param name="XDocument">the loaded xdocument to parse</param>
        /// <param name="events">the collection to save back to</param>
        /// <param name="filename">for error logging</param>
        internal void ParseXDocToEvents(XDocument dataDoc, ObservableCollection<Event> events, string filename)
        {

            try
            {
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

            }
            catch (Exception e)
            {
                // catch any exceptions to prevent a single bad file from crashing the app.
                LittleWatson.ReportException(e, "exception reading schedule from " + filename);
            }

        }       

        /// <summary>
        /// internal class for passing around version number/data/filename set of info.
        /// </summary>
        internal class ScheduleVersionData
        {
            public string filename;
            public DateTime creationDate;
            public int versionNumber;
            public string fileDownloadLocation;

            public ScheduleVersionData()
            {
            }

            public ScheduleVersionData(string filename)
            {
                this.filename = filename;
            }

        }


        /// <summary>
        /// parse out the data from a version or contents file into a scheduleData object for easy use
        /// </summary>
        /// <param name="versionDoc">the XDocument created from a version file</param>
        /// <param name="scheduleData">the class to store all the data in</param>
        internal void parseXDocToScheduleVersionData(XDocument versionDoc, ScheduleVersionData scheduleData)
        {
            // <version date = "3/9/2013" fileLocation = "http://paxwp7.nfshost.com/PAXEast/schedule.zip" number ="2"/>
            var versionData = versionDoc.Descendants("version").FirstOrDefault();
            if (versionData != null)
            {
                string date = versionData.Attribute("date").Value;
                scheduleData.creationDate = safeParse(date, "none", scheduleData.filename);
                Int32.TryParse(versionData.Attribute("number").Value, out scheduleData.versionNumber);
                scheduleData.fileDownloadLocation = versionData.Attribute("fileLocation").Value;
            }
            if (scheduleData.versionNumber == 0)    // bad version number? 
            {
                LittleWatson.ReportException(new Exception("file " + scheduleData.filename + " says version = 0"), versionDoc.ToString());
            }
        }


        /// <summary>
        /// given an xdoc of a contents.xml file, return a list of filename strings found in it
        /// </summary>
        /// <param name="readFromXap">tells if we need to modify the filenames to read them from xap storage</param>
        /// <param name="document"></param>
        /// <returns></returns>
        internal List<String> ReadFilenamesFromXdoc(bool readFromXap, XDocument document)
        {
            List<String> filenames = new List<string>();
            var files = from item in document.Descendants("file")
                    select new NamedItem
                    {
                        name = item.Attribute("name").Value
                    };
        
            foreach (var file in files)
            {
                string name = readFromXap ? "XML\\" + file.name : file.name;
                filenames.Add(name);
            }
            return filenames;
        }

        #endregion

        #region filename->xdoc parsing
        /// <summary>
        /// Open contents.xml to read the date that this schedule was created, so we know whether the server copy is more recent
        /// </summary>
        /// <param name="readFromXap"></param>
        /// <param name="filename"></param>
        /// <returns>Xdocument, may be null</returns>
        internal XDocument GetXDocFromFilename(bool readFromXap, string filename = null)
        {
            XDocument dataDoc = null;
            if (filename == null)
            {
                filename = "XML\\contents.xml";
            }
            try
            {
                if (readFromXap)
                {
                    dataDoc = XDocument.Load(filename);
                }
                else
                {
                    IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication();
                    Stream filestream = new IsolatedStorageFileStream(filename, System.IO.FileMode.Open, store);
                    dataDoc = XDocument.Load(filestream);
                    filestream.Close();
                }
            }
            catch (Exception e)
            {
                LittleWatson.ReportException(e, "(possible bad xml) exception reading from " + filename);
            }
            return dataDoc;
        }
        
        /// <summary>
        /// internal class for retrieving strings from xml docs using linq
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
    public class ScheduleDownloadEventArgs : EventArgs
    {
        public bool Success { get; set; }

        public ScheduleDownloadEventArgs(bool status)
        {
            Success = status;
        }
    }


}