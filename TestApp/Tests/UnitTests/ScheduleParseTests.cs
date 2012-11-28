using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Shapes;

using WindowsPhoneEssentials.Testing;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using PAX7.Model;
using PAX7.ViewModel;

using System.Collections.Generic; //list<string>
using System.Collections.ObjectModel; //observablecollection

namespace PAX7.Tests
{
    [TestClass]
    public class ScheduleParseTests 
    {

        [TestInitialize] //note, this will run before every test: I can't just run it for particular ones
        public void TestInit()
        {
        }
        
        [TestMethod]
        // method to confirm that the test framework is running
        public void TestAlwaysPasses()
        {
            Assert.IsTrue(true);
        }

        
        [TestMethod]
        // method to confirm the initialiser doesn't fail: if it does, all the tests will silently pass :O
        public void SmokeTest()
        {
            TestInit();
        }

        [TestMethod]
        [TestProperty("TestCategory", "Constructors")]
        public void CreateNewSchedule()
        {
            var testSchedule = new Schedule();
            Assert.IsNotNull(testSchedule);
            var emptyEvents = new ObservableCollection<Event>();
            Assert.Equals(emptyEvents.ToString(), testSchedule.Events.ToString()); //hack for not having an equals implemented on the generic collections
        }


        // creating schedule: read data from xml file and validate schedule Object is created
        [TestMethod]
        [TestProperty("TestCategory", "IO")]
        public void VerifyParsingGoodXMLToSchedule()
        {
            List<string> filenames = new List<string>();
            filenames.Add("Tests\\Data\\GoodXML.xml");
            var schedule = new Schedule();
            schedule.GetXMLEvents(filenames, true); //isFirstRun
            Assert.IsNotNull(schedule.Events, "event collection");
            var _emptyEvents = new ObservableCollection<Event>();
            Assert.AreNotEqual(_emptyEvents, schedule.Events);
        }

        /// <summary>
        ///  creating schedule: read data from xml file and validate the correct events were added to the schedule Object
        /// </summary>
        [TestMethod]
        [TestProperty("TestCategory", "IO")]
        public void VerifyParsingGoodXMLToScheduleEvents()
        {
            List<string> filenames = new List<string>();
            filenames.Add("Tests\\Data\\GoodXML.xml");
            var schedule = new Schedule();
            schedule.GetXMLEvents(filenames, true); //isFirstRun
            Assert.IsNotNull(schedule.Events, "event collection");
            var enumerator = schedule.Events.GetEnumerator();
            enumerator.MoveNext();
            var firstEvent = enumerator.Current;
            Assert.IsNotNull(firstEvent, "first event in collection");
            Assert.AreEqual("Contest", firstEvent.Kind);
            Assert.AreEqual("First Test Event", firstEvent.Name);
            Assert.AreEqual(false, firstEvent.Star);
        }

        #region error handling
        //TODO: all these failures should log different error messages, and the tests should confirm this

        /// <summary>
        ///  don't choke on bad xml file, make sure no exception made it out of the method
        /// </summary>
        [TestMethod]
        [TestProperty("TestCategory", "IO")]
        public void VerifyParsingBadXMLToSchedule()
        {
            List<string> filenames = new List<string>();
            filenames.Add("Tests\\Data\\BadXML_ampersand.xml");
            filenames.Add("Tests\\Data\\BadXML_empty.xml");
            var schedule = new Schedule();
            var _emptyEvents = new ObservableCollection<Event>();
            schedule.GetXMLEvents(filenames, true); //isFirstRun
            Assert.IsNotNull(schedule.Events, "confirming event collection exists");
            // malformed events should not be added to the schedule
            Assert.Equals(_emptyEvents, schedule.Events);
        }

        /// <summary>
        ///  don't choke on bad event definitions, make sure no exception made it out of the method
        /// </summary>
        [TestMethod]
        [TestProperty("TestCategory", "IO")]
        public void VerifyParsingBadEventsToSchedule()
        {
            List<string> filenames = new List<string>();
            filenames.Add("Tests\\Data\\BadXML_missingKind.xml");
            filenames.Add("Tests\\Data\\BadXML_missingName.xml");
            filenames.Add("Tests\\Data\\BadXML_missingLocation.xml");
            filenames.Add("Tests\\Data\\BadXML_missingDatetime.xml");
            filenames.Add("Tests\\Data\\BadXML_missingDescription.xml");
            filenames.Add("Tests\\Data\\BadXML_missingEnd.xml");
            var schedule = new Schedule();
            var _emptyEvents = new ObservableCollection<Event>();
            schedule.GetXMLEvents(filenames, true); //isFirstRun
            Assert.IsNotNull(schedule.Events, "confirming event collection exists");
            // malformed events should not be added to the schedule
            Assert.Equals(_emptyEvents, schedule.Events);
        }

        /// <summary>
        ///  don't choke on bad date formats - events should be created with a fake date 
        /// </summary>
        [TestMethod]
        [TestProperty("TestCategory", "IO")]
        public void VerifyParsingBadDatesToSchedule()
        {
            List<string> filenames = new List<string>();
            filenames.Add("Tests\\Data\\GoodXML_malformedDate.xml");
            var schedule = new Schedule();
            var _emptyEvents = new ObservableCollection<Event>();
            schedule.GetXMLEvents(filenames, true); //isFirstRun
            Assert.IsNotNull(schedule.Events);
            //events with malformed dates should be added to the schedule with a fake date
            Assert.AreNotEqual(_emptyEvents, schedule.Events);
            Assert.IsNotNull(schedule.Events, "event collection");
            var enumerator = schedule.Events.GetEnumerator();
            enumerator.MoveNext();
            var firstEvent = enumerator.Current;
            Assert.IsNotNull(firstEvent, "first event in collection");
            Assert.AreEqual("Panel", firstEvent.Kind);
            Assert.AreEqual("Datetime 08272011 110000", firstEvent.Name);
            Assert.IsNotNull(firstEvent.StartTime, "datetime");
            Assert.AreEqual("Saturday 9:15 PM", firstEvent.friendlyStartTime);
            Assert.AreEqual(false, firstEvent.Star);
            enumerator.MoveNext();
            var secondEvent = enumerator.Current;
            Assert.IsNotNull(secondEvent, "first event in collection");
            Assert.AreEqual("Panel", secondEvent.Kind);
            Assert.IsNotNull(secondEvent.StartTime, "datetime");
            Assert.AreEqual("Saturday 9:15 PM", secondEvent.friendlyStartTime);
            Assert.AreEqual("Datetime Friday 10 November", secondEvent.Name);
            Assert.AreEqual(false, secondEvent.Star);
            //check for little watson file in isolated storage?
        }

        /// <summary>
        /// Referencing a missing xml file should not break the app - no exceptions thrown
        /// </summary>
        [TestMethod]
        [TestProperty("TestCategory", "IO")]
        public void VerifyParsingMissingXMLToSchedule()
        {
            List<string> filenames = new List<string>();
            filenames.Add("Tests\\Data\\BadXML_filedoesnotexist.xml");
            var schedule = new Schedule();
            var _emptyEvents = new ObservableCollection<Event>();
            schedule.GetXMLEvents(filenames, true); //isFirstRun
            Assert.IsNotNull(schedule.Events, "confirming event collection exists");
            //no file so no events should have been created
            Assert.Equals(_emptyEvents, schedule.Events);
        }

        #endregion error handling

        /// <summary>
        /// verify that all my real xml files are read properly
        /// </summary>


        ///<summary>
        /// Save events to isolated storage
        /// </summary>


        /// <Summary>
        /// retrieve schedule from isolated storage - verify they match what I saved in, especially stars 
        /// </Summary> 
        [TestMethod]
        public void ReadEventsFromStorage()
        {
            var testSchedule = new Schedule();
            testSchedule.SaveEvents();
            testSchedule.GetSavedEvents();
        }
        

    }
}
