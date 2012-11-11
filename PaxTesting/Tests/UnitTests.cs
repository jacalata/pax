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
using PAX7.View; //uhoh
using PAX7.ViewModel;

using System.Collections.Generic; //list<string>
using System.Collections.ObjectModel; //observablecollection

namespace PAX7.Tests
{
    [TestClass]
    public class UnitTests 
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

        /*
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
        }

        [TestMethod]
        // why doesn't the [Tag("thing")] work? missing a using or something?
        public void CreateNewEvent()
        {
            var testEvent = new Event();
            Assert.IsNotNull(testEvent);
        }

        // creating schedule: read data from xml file and validate schedule Object is created
        [TestMethod]
        [TestProperty("TestCategory", "IO")]
        public void VerifyParsingGoodXMLToSchedule()
        {
            List<string> filenames = new List<string>();
            filenames.Add("Tests\\Data\\GoodXML.xml");
            var schedule = new Schedule();
            schedule.GetXMLEvents(filenames);
            Assert.IsNotNull(schedule.Events, "event collection");
            var _emptyEvents = new ObservableCollection<Event>();
            Assert.AreNotEqual(schedule.Events, _emptyEvents);
        }

        // creating schedule: read data from xml file and validate the correct events were added to the schedule Object
        [TestMethod]
        [TestProperty("TestCategory", "IO")]
        public void VerifyParsingGoodXMLToScheduleEvents()
        {
            List<string> filenames = new List<string>();
            filenames.Add("Tests\\Data\\GoodXML.xml");
            var schedule = new Schedule();
            schedule.GetXMLEvents(filenames);
            Assert.IsNotNull(schedule.Events, "event collection");
            var enumerator = schedule.Events.GetEnumerator();
            enumerator.MoveNext();
            var firstEvent = enumerator.Current;
            Assert.IsNotNull(firstEvent, "first event in collection");
            Assert.AreEqual(firstEvent.Kind, "Contest");
            Assert.AreEqual(firstEvent.Name, "First Test Event");
            Assert.AreEqual(firstEvent.Star, false);
            //datetime, end, location
        }

        // don't choke on bad xml file, make sure no exception made it out of the method
        [TestMethod]
        [TestProperty("TestCategory", "IO")]
        public void VerifyParsingBadXMLToSchedule()
        {
            List<string> filenames = new List<string>();
            filenames.Add("BadXML_ampersand.xml");
            filenames.Add("BadXML_missingKind.xml");
            filenames.Add("BadXML_missingName.xml");
            var schedule = new Schedule();
            var _emptyEvents = new ObservableCollection<Event>();
            schedule.GetXMLEvents(filenames);
            Assert.IsNotNull(schedule.Events);
            Assert.Equals(schedule.Events, _emptyEvents);
        }

        //verify that all my xml files are read properly

        // add note to event
        // edit note on event
        // delete/clear note from event

        [TestMethod]
        [TestProperty("TestCategory", "Constructors")]
        public void createScheduleViewModel()
        {
            var schedule = new Schedule();
            //need a fake pivotView
            var pivotView = new SchedulePivotView(); //haven't implemented a fake one of this yet
            var pivotParam = "byDay"; // pivotView.pivotString;
            var viewModel = new ScheduleViewModel(pivotView, pivotParam);
            Assert.IsNotNull(viewModel);
        }

        [TestMethod]
        [TestProperty("TestCategory", "Constructors")]
        public void CreateSchedulePivotView()
        {
            Assert.Inconclusive("not implemented yet");
        }
        */

    }
}
