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

using Microsoft.Silverlight.Testing.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PAX7.Model;
using PAX7.View; //uhoh
using PAX7.ViewModel;
using PAX7.Tests.Fakes;

using System.Collections.Generic; //list<string>
using System.Collections.ObjectModel; //observablecollection

namespace PAX7.Tests
{
    [TestClass]
    public class ScheduleTests
    {
     
        /*
        private Schedule schedule = new Schedule();
        
        [TestInitialize]
        public void _createAndPopulateSchedule()
        {
            // populate the isolated storage

            List<string> filenames = new List<string>();
            filenames.Add("Tests\\Data\\GoodXML.xml");
            schedule.GetXMLEvents(filenames);
            Assert.IsNotNull(schedule.Events, "event collection");
            var _emptyEvents = new ObservableCollection<Event>();
            Assert.AreNotEqual(schedule.Events, _emptyEvents);
        }

        [TestMethod]
        public void CompareEmptySets()
        {
            var sched = new Schedule();
            var _emptyEvents = new ObservableCollection<Event>();
            Assert.AreEqual(sched.Events, _emptyEvents);
        }

        [TestMethod]
        public void AddEventToPersonalSet()
        {
            var enumerator = schedule.Events.GetEnumerator();
            enumerator.MoveNext();
            Event firstEvent = enumerator.Current;
            // save details of this event
            string eventName = firstEvent.Name;
            string eventKind = firstEvent.Kind;
            string eventLocation = firstEvent.Location;
            string eventDate = firstEvent.friendlyStartTime;
            Assert.IsFalse(firstEvent.Star);
            firstEvent.Star = true;
        // need to assert that the property changed event was raised. This test should actually be in an Event unit test class

            SchedulePivotView pivotView = new SchedulePivotView(); //haven't implemented a fake one of this yet
            string pivotParam = ScheduleViewModel.PivotView.Stars.ToString(); // pivotView.pivotString;
            var viewModel = new ScheduleViewModel(pivotView, pivotParam);
            Assert.IsNotNull(viewModel);
            viewModel.LoadSchedule();
            foreach (var slice in viewModel.EventSlices)
            {
                Assert.IsNotNull(slice);
            }
            var sliceEnumerator = viewModel.EventSlices.GetEnumerator();// returns a set of event slices
            sliceEnumerator.MoveNext();
            var eventSlices = sliceEnumerator.Current; 

            var faveEvents = eventSlices.events.GetEnumerator(); // returns a set of events
            faveEvents.MoveNext();
            Event faveEvent = faveEvents.Current;
            Assert.IsNotNull(faveEvent);

            Assert.AreEqual(faveEvent.Location, eventLocation);
            Assert.AreEqual(faveEvent.Kind, eventKind);
            Assert.AreEqual(faveEvent.Name, eventName);
            Assert.AreEqual(faveEvent.Star, true);
            Assert.AreEqual(faveEvent.friendlyStartTime, eventDate);
            
        }

        [TestMethod]
        public void RemoveEventFromPersonalSet()
        {
            Assert.Inconclusive("not implemented yet");

        }


        [TestMethod]
        [TestProperty("TestCategory", "PostUpdate")]
        public void VerifyPersonalScheduleAfterUpdate()
        {
            //need to have stored a copy of the data before updating, then compare to it 
            Assert.Inconclusive("not implemented yet");
        }

        [TestMethod]
        [TestProperty("TestCategory", "PreUpdate")]
        public void SavePersonalScheduleBeforeUpdate()
        {
            schedule.SaveEvents();
            //validate isolated storage here
        }


        //safeParse method
        //saveEvents method: check that it maintains stars on all events
        
        // save schedule to isolated storage

        // retrieve schedule from isolated storage
        [TestMethod]
        public void ReadEventsFromStorage()
        {
            schedule.SaveEvents();
            schedule.GetSavedEvents();

        }


        #region UI Tests?
        [TestMethod]
        public void ViewScheduleByDay()
        {
            Assert.Fail("not implemented yet");
        }

        [TestMethod]
        public void ViewScheduleByRoom()
        {
            Assert.Inconclusive("not implemented yet");
        }

        [TestMethod]
        public void ViewScheduleByType()
        {
            Assert.Inconclusive("not implemented yet");
        }

        [TestMethod]
        public void ViewPersonalSchedule()
        {
            Assert.Inconclusive("not implemented yet");
        }

        #endregion
         */
    }
}
