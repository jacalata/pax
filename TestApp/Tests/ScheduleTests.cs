using System;
using System.Collections.Generic; //list<string>
using System.Collections.ObjectModel; //observablecollection
using System.IO.IsolatedStorage;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PAX7.Model;

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

        /* commenting out while tracking down the test that runs forever
        public void VerifyEventFromFailedCheckForUpdate()
        {            
            _schedule.evt_updateCheckComplete += delegate(object Sender, EventArgs e)
            {
                _callbackDone = true;
            };
            string 404uri = "http://paxwp7.nfshost.com/thiswillreturna404error.txt";
            _schedule.checkForNewSchedule(404uri); // this will trigger webClient_VersionInfoCompleted which will raise the event evt_updateCheckComplete
            // wait for our method to trigger on the event and set this to true
            EnqueueConditional(() => _callbackDone);
            EnqueueTestComplete();
         }
         
        // now that we're reading the schedule locations from xml, we have to make sure
        // that updated location lists in a schedule update get read
        public void VerifyThatNewScheduleLocationsAreRecordedFromUpdatedSchedule()
        {
          //load with East Data
         * // update to Prime data
         * check eventLocations in Schedule.cs
        }

        /// <summary>
        /// verify default values when we have no record of receiving an update
        /// </summary>
         [TestMethod]
        [Tag("schedule")]
        public void VerifyDefaultUpdateRecord()
        {
           // bool localLastUpdate = true;
          //  IsolatedStorageSettings.ApplicationSettings.TryGetValue(_schedule.IsoStoreLastUpdatedRecord, out localLastUpdate);
            //Assert.AreEqual("false", localLastUpdate);
            Assert.IsFalse(VerifyUpdateAvailable());
        }

        /// <summary>
        /// verify successful response when we have never had an update and the server is newer
        /// </summary>
         [TestMethod]
        [Asynchronous]
        [Tag("schedule")]
        public void VerifyResponseToSuccessfulCheckForUpdate()
        {
            bool localLastUpdate = true;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue(_schedule.IsoStoreLastUpdatedRecord, out localLastUpdate);
            Assert.IsFalse(localLastUpdate);
            _schedule.evt_updateCheckComplete += delegate(object Sender, EventArgs e)
            {
                _callbackDone = true;
            };
            _schedule.checkForNewSchedule(); // this will trigger webClient_VersionInfoCompleted which will raise the event evt_updateCheckComplete
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
        public void VerifyResponseToCheckForUpdateWhenLocalIsOlder()
        {
            _schedule.RecordScheduleCreationDate(true, "\\Tests\\Data\\GoodContents_OldDate.xml"); //3-31-2000
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
        public void VerifyResponseToCheckForUpdateWhenLocalIsNewer()
        {
            _schedule.RecordScheduleCreationDate(true, "\\Tests\\Data\\GoodContents_RecentDate.xml"); //11-31-2012
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
        public void VerifyResponseToCheckForUpdateNoVersionFileExists()
        {
            _schedule.RecordScheduleCreationDate(true, "\\Tests\\Data\\GoodContents_RecentDate.xml"); //11-31-2012
            _schedule.evt_updateCheckComplete += delegate(object Sender, EventArgs e)
            {
                _callbackDone = true;
            };
            _schedule.checkForNewSchedule(@"http://paxwp7.nfshost.com/test/nothingisatthislocation.txt");
            EnqueueConditional(() => _callbackDone);
            EnqueueCallback(() => Assert.IsTrue(VerifyUpdateAvailable()));
            EnqueueTestComplete();
        }

        /// <summary>
        /// verify recording the schedule creation date
        /// </summary>
         [TestMethod]
        [Tag("schedule")]
        public void VerifyRecordScheduleCreationDate()
        {
            _schedule.RecordScheduleCreationDate(true, "Tests\\Data\\contents.xml");
            Assert.IsTrue(IsolatedStorageSettings.ApplicationSettings.Contains(_schedule.IsoStoreLastUpdatedRecord));
            DateTime parsedDate;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue(_schedule.IsoStoreLastUpdatedRecord, out parsedDate);
            Assert.IsInstanceOfType(parsedDate, typeof(DateTime));
            Assert.Equals(parsedDate, DateTime.Parse("3/31/2012"));
        }

        /// <summary>
        /// verify recording the schedule creation date over another date
        /// </summary>
         [TestMethod]
        [Tag("schedule")]
        public void VerifyRecordScheduleCreationDateOverAnother()
        {
            IsolatedStorageSettings.ApplicationSettings.Add(_schedule.IsoStoreLastUpdatedRecord, DateTime.Parse("1/1/2000"));
            _schedule.RecordScheduleCreationDate(true, "Tests\\Data\\contents.xml");
            Assert.IsTrue(IsolatedStorageSettings.ApplicationSettings.Contains(_schedule.IsoStoreLastUpdatedRecord));
            DateTime parsedDate;
            IsolatedStorageSettings.ApplicationSettings.TryGetValue(_schedule.IsoStoreLastUpdatedRecord, out parsedDate);
            Assert.IsInstanceOfType(parsedDate, typeof(DateTime));
            Assert.Equals(parsedDate, DateTime.Parse("3/31/2012"));
        }

        /// <summary>
        /// verify nothing was saved and exception was thrown if there was no/bad date in contents.xml 
        /// </summary>
         [TestMethod]
        [Tag("schedule")]
        public void VerifyRecordScheduleCreationDateFailed()
        {
            _schedule.NukeAllStorage();
            _schedule.RecordScheduleCreationDate(true, "Tests\\Data\\BadContents_NoDate.xml");
            Assert.IsFalse(IsolatedStorageSettings.ApplicationSettings.Contains(_schedule.IsoStoreLastUpdatedRecord));
        }


        /// <summary>
        /// Verify that downloading new schedule throws event
        /// </summary>
         [TestMethod]
        [Asynchronous]
        [Tag("schedule")]
        public void VerifyEventFromDownloadingUpdate()
        {
            _schedule.DownloadNewEventFiles(); 
            _schedule.evt_downloadScheduleComplete += delegate(object Sender, EventArgs e)
            {
                _callbackDone = true;
            };
            EnqueueConditional(() => _callbackDone);
            EnqueueTestComplete();
        }


        /// <summary>
        /// verify that schedule is updated with data from downloaded file
        /// </summary>
        // [TestMethod]
        [Asynchronous]
        [Tag("schedule")]
        public void VerifyAddEventsToScheduleByUpdate()
        {
            string serverLocation = "http://paxwp7.nfshost.com/East/2012/schedule.zip"; //need to put up a separate test zip
            _schedule.DownloadNewEventFiles(serverLocation);
            _schedule.evt_downloadScheduleComplete += delegate(object Sender, EventArgs e)
            {
                _callbackDone = true;
            };
            EnqueueConditional(() => _callbackDone);
            EnqueueCallback(() => Assert.IsTrue(FindEventInSchedule("Season Four Fast Four Fourious")));  
            // check the update date in isostore?
            EnqueueTestComplete();
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

         * */


        #region helpers



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
                if (true == IsolatedStorageSettings.ApplicationSettings.TryGetValue(_schedule.IsoStoreHasUpdateAvailable, out localHasUpdate))
                {
                    return localHasUpdate;
                }
                else
                {
                    return false;
                }
            }
            catch (Exception e)
            {
                return false;
            }
        }

        /// <summary>
        /// helper method to delete all global values in the schedule and clear the storage.
        /// </summary>
        private void WipeSchedule()
        {
            _schedule.Events = null;
            _schedule.eventLocations = null;
            _schedule.eventDays = null;
            _schedule.eventTypes = null;
            _schedule.NukeAllStorage();
            _schedule.hasUpdateAvailable = false;
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
            _schedule.GetXMLEvents(true, filenames); //read from xap
            Assert.IsNotNull(_schedule.Events, "event collection");
            var _emptyEvents = new ObservableCollection<Event>();
            Assert.AreNotEqual(_schedule.Events, _emptyEvents);
            _schedule.SaveEvents(); // to isolated storage
        }

        /// <summary>
        /// helper method to check that an event is not in the schedule
        /// </summary>
        /// <param name="eventName"></param>
        /// <returns></returns>
        private bool FindEventInSchedule(string eventName)
        {
            bool found = false;
            IEnumerator<Event> events = _schedule.Events.GetEnumerator();
            do
            {
                found = events.Current.Name.Equals(eventName);
                if (found == true)
                    break;
            }
            while (events.MoveNext()); // returns false when it reaches past the last element            
            return found;
        }

        #endregion

    }
}
