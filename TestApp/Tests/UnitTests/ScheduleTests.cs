using System;
using System.Collections.Generic; //list<string>
using System.Collections.ObjectModel; //observablecollection
using System.IO.IsolatedStorage;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PAX7.Model;
using PAX7.Utilicode; //iso store settings
using System.Xml;
using System.Xml.Linq;

namespace PAX7.Tests
{
    [TestClass]
    public class ScheduleTests : SilverlightTest
    {

        private Schedule _schedule;
        private bool _callbackDone = false;


        [TestInitialize]
        public void TestInitialize()
        {
            _callbackDone = false;
            _schedule = new Schedule(true); // create it with test flag
            _schedule.NukeAllStorage();
        }

        [TestCleanup]
        public void TestCleanup()
        {
            _schedule.NukeAllStorage();
        }


        /// <summary>
        /// Verify that checking for an updated schedule throws an event
        /// </summary>
        [TestMethod]
        [Asynchronous]
        [Tag("schedule")]
        [Tag("async")]
        public void VerifyEventFromCheckForUpdate()
        {
            _schedule.evt_updateCheckComplete += delegate(object Sender, EventArgs e)
            {
                _callbackDone = true;
            };
            _schedule.checkForNewSchedule(); // this will trigger webClient_VersionInfoCompleted which will raise the event evt_updateCheckComplete
            // wait for our method to trigger on the event and set this to true
            EnqueueConditional(() => _callbackDone);
            EnqueueTestComplete();
        }
               
        /// <summary>
        /// Verify that checking for an updated schedule throws an event even if it 404d or errord
        /// </summary>
        [TestMethod]
        [Asynchronous]
        [Tag("schedule")]
        [Tag("async")]
        public void VerifyEventFromFailedCheckForUpdate()
        {            
            _schedule.evt_updateCheckComplete += delegate(object Sender, EventArgs e)
            {
                _callbackDone = true;
            };
            string erroruri = "http://paxwp7.nfshost.com/thiswillreturna404error.txt";
            _schedule.checkForNewSchedule(erroruri); // this will trigger webClient_VersionInfoCompleted which will raise the event evt_updateCheckComplete
            // wait for our method to trigger on the event and set this to true
            EnqueueConditional(() => _callbackDone);
            EnqueueTestComplete();
         }
         
        /*
        // now that we're reading the schedule locations from xml, we have to make sure
        // that updated location lists in a schedule update get read
        public void VerifyThatNewScheduleLocationsAreRecordedFromUpdatedSchedule()
        {
          //load with East Data
         * // update to Prime data
         * check eventLocations in Schedule.cs
        }
        */
        /// <summary>
        /// verify default values when we have no record of receiving an update
        /// </summary>
         [TestMethod]
        [Tag("schedule")]
        public void VerifyDefaultUpdateRecord()
        {
             Assert.IsFalse(VerifyUpdateAvailable());
        }

        /// <summary>
        /// verify successful response when we have never had an update and the server is newer
        /// </summary>
         [TestMethod]
        [Asynchronous]
         [Tag("schedule")]
         [Tag("async")]
        public void VerifyResponseToSuccessfulCheckForUpdate()
        {
            bool localLastUpdate = true;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue(IsoStoreSettings.IsoStoreLastUpdatedRecord, out localLastUpdate);
            Assert.IsFalse(localLastUpdate);
            IsolatedStorageSettings.ApplicationSettings.Add(IsoStoreSettings.IsoStoreScheduleVersionNumber, 0); //super low value
            _schedule.evt_updateCheckComplete += delegate(object Sender, EventArgs e)
            {
                _callbackDone = true;
            };
            _schedule.checkForNewSchedule(@"http://paxwp7.nfshost.com/test/veryNewSchedule.txt"); // this will trigger webClient_VersionInfoCompleted which will raise the event evt_updateCheckComplete
            // wait for our method to trigger on the event and set this to true
            EnqueueConditional(() => _callbackDone);
            EnqueueCallback(() => Assert.IsTrue(VerifyUpdateAvailable())); 
            EnqueueTestComplete();
        }

       
        /// <summary>
        ///verify response when we have had an update before but the server is still newer
        /// </summary>
         [TestMethod]
        [Asynchronous]
         [Tag("schedule")]
         [Tag("async")]
        public void VerifyResponseToCheckForUpdateWhenLocalIsOlder()
        {
             bool readFromXap = true;
             XDocument xdoc = _schedule.GetXDocFromFilename(readFromXap, "Tests\\Data\\GoodContents_OldDate.xml"); //3-31-2000
             PAX7.Model.Schedule.ScheduleVersionData scheduleData = new PAX7.Model.Schedule.ScheduleVersionData();
             _schedule.parseXDocToScheduleVersionData(xdoc, scheduleData);
             _schedule.SaveScheduleVersionDataToIsoStore(scheduleData);
            _schedule.evt_updateCheckComplete += delegate(object Sender, EventArgs e)
            {
                _callbackDone = true;
            };
            _schedule.checkForNewSchedule(@"http://paxwp7.nfshost.com/test/veryNewSchedule.txt"); 
            EnqueueConditional(() => _callbackDone);
            EnqueueCallback(() => Assert.IsTrue(VerifyUpdateAvailable()));
            EnqueueTestComplete();
        }

         /// <summary>
         /// verify response to check when there is a schedule available but we have a newer copy already 
         /// </summary>
          [TestMethod]
         [Asynchronous]
         [Tag("schedule")]
         [Tag("async")]
         public void VerifyResponseToCheckForUpdateWhenLocalIsNewer()
         {
             bool readFromXap = true;
             XDocument xdoc = _schedule.GetXDocFromFilename(readFromXap, "Tests\\Data\\GoodContents_RecentDate.xml"); //11-31-2012
             PAX7.Model.Schedule.ScheduleVersionData scheduleData = new PAX7.Model.Schedule.ScheduleVersionData();
             _schedule.parseXDocToScheduleVersionData(xdoc, scheduleData);
             _schedule.SaveScheduleVersionDataToIsoStore(scheduleData);
             _schedule.evt_updateCheckComplete += delegate(object Sender, EventArgs e)
             {
                 _callbackDone = true;
             };
             _schedule.checkForNewSchedule(@"http://paxwp7.nfshost.com/test/reallyOldSchedule.txt");
             EnqueueConditional(() => _callbackDone);
             EnqueueCallback(() => Assert.IsTrue(VerifyUpdateAvailable()));
             EnqueueTestComplete();
         }

         /// <summary>
         /// verify response when there is no update on the server
         /// </summary>
          [TestMethod]
         [Asynchronous]
          [Tag("schedule")]
          [Tag("async")]
         public void VerifyResponseToCheckForUpdateVersionFileIsNewerButScheduleNotFound()
         {              
             bool readFromXap = true;
             XDocument xdoc = _schedule.GetXDocFromFilename(readFromXap, "Tests\\Data\\GoodContents_RecentDate.xml"); //11-31-2012
             PAX7.Model.Schedule.ScheduleVersionData scheduleData = new PAX7.Model.Schedule.ScheduleVersionData();
             _schedule.parseXDocToScheduleVersionData(xdoc, scheduleData);
             _schedule.SaveScheduleVersionDataToIsoStore(scheduleData);
              
             _schedule.evt_updateCheckComplete += delegate(object Sender, EventArgs e)
             {
                 _callbackDone = true;
             };
             _schedule.checkForNewSchedule(@"http://paxwp7.nfshost.com/test/nothingisatthislocation.txt");
             EnqueueConditional(() => _callbackDone);
             EnqueueCallback(() => Assert.IsFalse(VerifyUpdateAvailable()));
             EnqueueTestComplete();
         }

          /// <summary>
          /// verify recording the schedule creation date
          /// </summary>
           [TestMethod]
          [Tag("schedule")]
          public void VerifyReadContentsXMLForCreationDate()
          {
              bool readFromXap = true;
              XDocument xdoc = _schedule.GetXDocFromFilename(readFromXap, "Tests\\Data\\contents.xml");
              PAX7.Model.Schedule.ScheduleVersionData scheduleData = new PAX7.Model.Schedule.ScheduleVersionData();
              _schedule.parseXDocToScheduleVersionData(xdoc, scheduleData);
              _schedule.SaveScheduleVersionDataToIsoStore(scheduleData);
              Assert.IsTrue(IsolatedStorageSettings.ApplicationSettings.Contains(IsoStoreSettings.IsoStoreScheduleCreationDate));
              DateTime parsedDate;
              IsolatedStorageSettings.ApplicationSettings.TryGetValue(IsoStoreSettings.IsoStoreScheduleCreationDate, out parsedDate);
              Assert.IsInstanceOfType(parsedDate, typeof(DateTime));
              Assert.Equals(parsedDate, DateTime.Parse("3/31/2012"));
          }

          /// <summary>
          /// verify recording the schedule creation date over another date
          /// </summary>
           [TestMethod]
          [Tag("schedule")]
        [Tag("now")]
          public void VerifyRecordScheduleCreationDateOverAnother()
          {
              IsolatedStorageSettings.ApplicationSettings.Add(IsoStoreSettings.IsoStoreScheduleCreationDate, DateTime.Parse("1/1/2000"));
              bool readFromXap = true;
              XDocument xdoc = _schedule.GetXDocFromFilename(readFromXap, "Tests\\Data\\contents.xml");
              PAX7.Model.Schedule.ScheduleVersionData scheduleData = new PAX7.Model.Schedule.ScheduleVersionData();
              _schedule.parseXDocToScheduleVersionData(xdoc, scheduleData);
              _schedule.SaveScheduleVersionDataToIsoStore(scheduleData);
              Assert.IsTrue(IsolatedStorageSettings.ApplicationSettings.Contains(IsoStoreSettings.IsoStoreScheduleCreationDate));
              DateTime parsedDate;
              IsolatedStorageSettings.ApplicationSettings.TryGetValue(IsoStoreSettings.IsoStoreScheduleCreationDate, out parsedDate);
              Assert.IsInstanceOfType(parsedDate, typeof(DateTime));
              Assert.Equals(parsedDate, DateTime.Parse("3/31/2012"));
          }

          /// <summary>
          /// verify default values were saved if there was no/bad date in contents.xml 
          /// </summary>
           [TestMethod]
          [Tag("schedule")]
          public void VerifyRecordScheduleCreationDateFailed()
          {
               //TODO: I'm not really sure what I want to happen here. For now it creates a date of 0/0/01 and version no. 0

              bool readFromXap = true;
              XDocument xdoc = _schedule.GetXDocFromFilename(readFromXap, "Tests\\Data\\BadContents_NoDate.xml");
              PAX7.Model.Schedule.ScheduleVersionData scheduleData = new PAX7.Model.Schedule.ScheduleVersionData();
              _schedule.parseXDocToScheduleVersionData(xdoc, scheduleData);
              _schedule.SaveScheduleVersionDataToIsoStore(scheduleData);
               Assert.IsTrue(IsoStoreSettings.GetScheduleVersion() == 0);
               Assert.IsTrue(IsolatedStorageSettings.ApplicationSettings.Contains(IsoStoreSettings.IsoStoreScheduleCreationDate));
          }


           /// <summary>
           /// verify that schedule is updated with data from downloaded file
           /// </summary>
           [TestMethod]
           [Asynchronous]
           [Tag("schedule")]
           [Tag("async")]
        [Tag("update")]
           public void VerifyAddEventsToScheduleByUpdate()
           {
               string serverLocation = "http://paxwp7.nfshost.com/PAXEast/2012/schedule.zip"; 
               _schedule.DownloadNewEventFiles(serverLocation);
               _schedule.evt_downloadScheduleComplete += delegate(object Sender, ScheduleDownloadEventArgs e)
               {
                   _callbackDone = true;
               };
               EnqueueConditional(() => _callbackDone);
               EnqueueCallback(() => FindEvent("Season Four Fast Four Fourious"));  // this should be executed after the callback value becomes true
               // TODO: check the update date in isostore?
               EnqueueTestComplete();
           }

           [Asynchronous]
           [Tag("schedule")]
           [Tag("async")]
           [Tag("update")]
        public void VerifyNoStarredEventsAreLostInUpdate()
        {
            Assert.IsTrue(false);
        }


           // verify what happens when a file is missing or corrupt during update?

        /// <summary>
        /// Verify that events are correctly saved to Isolated Storage by reading them back
        /// </summary>
         [TestMethod]
        [Tag("schedule")]
        public void VerifyEventsSavedToIsolatedStorage()
        {
            CreateAndPopulateSchedule("Tests\\Data\\GoodXML.xml");

            Assert.IsTrue(IsolatedStorageSettings.ApplicationSettings.TryGetValue("eventLocations", out _schedule.eventLocations));
            Assert.IsTrue(IsolatedStorageSettings.ApplicationSettings.TryGetValue("eventDays", out _schedule.eventDays));
            Assert.IsTrue(IsolatedStorageSettings.ApplicationSettings.TryGetValue("eventTypes", out _schedule.eventTypes));

            ObservableCollection<Event> events = new ObservableCollection<Event>();
            foreach (Object o in IsolatedStorageSettings.ApplicationSettings.Values)
            {
                if (o is Event)
                    events.Add((Event)o);
            }
            Assert.IsNotNull(events);
            IEnumerator<Event> eventEnumerator =  events.GetEnumerator();
            eventEnumerator.MoveNext(); //must be done after creation
            Event firstEvent = eventEnumerator.Current;
            Assert.IsNotNull(firstEvent);
            Assert.Equals(firstEvent.Name, "First Test Event");
        }


        /// <Summary>
        /// retrieve schedule from isolated storage - 
        /// </Summary> 
        [TestMethod]
        public void ReadEventsFromStorage()
        {
            var testSchedule = new Schedule();
            CreateAndPopulateSchedule("Tests\\Data\\GoodXML.xml");

            var events = _schedule.GetSavedEvents();
            Assert.IsTrue(FindEventInSchedule(events, "First Test Event"));
            Assert.IsNotNull(_schedule.eventLocations);
            Assert.IsNotNull(_schedule.eventTypes);
            Assert.IsNotNull(_schedule.eventDays);
        }


        #region helpers


        private void FindEvent(string eventName)
        {
            ObservableCollection<Event> events = _schedule.GetSavedEvents();
            Assert.IsTrue(FindEventInSchedule(events, eventName));
        }

        /// <summary>
        /// helper method returns the value of HasUpdateAvailable from IsoStore
        /// swallows any exceptions during the read of IsoStore
        /// </summary>
        /// <returns></returns>
        private bool VerifyUpdateAvailable()
        {
            bool localHasUpdate;
            try
            {
                if (true == IsolatedStorageSettings.ApplicationSettings.TryGetValue(IsoStoreSettings.IsoStoreHasUpdateAvailable, out localHasUpdate))
                {
                    return localHasUpdate;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception) // will this ever throw an exception? 
            {
                return false;
            }
        }

        /// <summary>
        /// helper method to choose the test data and populate the schedule with it
        /// </summary>
        /// <param name="testDataFile"></param>
        private void CreateAndPopulateSchedule(string testDataFile)
        {
            // populate the isolated storage
            List<string> filenames = new List<string>();
            filenames.Add(testDataFile);
            var _Events = new ObservableCollection<Event>();
            _Events = _schedule.GetXMLEvents(true, filenames); //read from xap
            _schedule.SaveEvents(_Events); // to isolated storage
            // check the isolated storage...
        }


        /// <summary>
        /// helper method to check that an event is not in the schedule
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns></returns>
        private bool FindEventInSchedule(ObservableCollection<Event> Events, string eventName)
        {
            bool found = false;
            IEnumerator<Event> events = Events.GetEnumerator();
            while (!found && events.MoveNext()) // move to first element
            {
                found = events.Current.Name.Contains(eventName);
            }          
            return found;
        }

        #endregion

    }
}
