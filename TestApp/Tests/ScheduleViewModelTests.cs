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

using Microsoft.Silverlight.Testing;
using Microsoft.Silverlight.Testing.UnitTesting;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PAX7.Model;
using PAX7.ViewModel;
using PAX7.Tests.Fakes;

using System.Collections.Generic; //list<string>
using System.Collections.ObjectModel; //observablecollection

namespace PAX7.Tests
{
    [TestClass]
    public class ScheduleViewModelTests :SilverlightTest
    {
        
        private Schedule _schedule = new Schedule();
        private ScheduleViewModel MockViewModel;
        private Event _event;
        private bool _callbackDone = false;


        public void _createAndPopulateSchedule(string testDataFile)
        {
            // populate the isolated storage
            _schedule.NukeAllStorage();
            List<string> filenames = new List<string>();
            filenames.Add(testDataFile);
            _schedule.GetXMLEvents(true, filenames); //read from xap
            Assert.IsNotNull(_schedule.Events, "event collection");
            var _emptyEvents = new ObservableCollection<Event>();
            Assert.AreNotEqual(_schedule.Events, _emptyEvents);
            _schedule.SaveEvents(); // to isolated storage
        }

        [TestInitialize]
        public void _createAndPopulateSchedule()
        {
            MockViewModel = null;
        }

        [TestCleanup]
        public void _clearSchedule()
        {
            _callbackDone = false;
            MockViewModel = null;
            _event = null;
            _schedule.Clear();
        }


        [TestMethod]
        [TestProperty("TestCategory", "Constructors")]
        public void createScheduleViewModel()
        {
            var schedule = new Schedule();
            string pivotType = "byDay"; 
            var viewModel = new ScheduleViewModel(null, pivotType);
            Assert.IsNotNull(viewModel);
        }

        /*
        [TestMethod]
        [ExpectedException(typeof(AssertInconclusiveException))]
        public void ViewScheduleByDay()
        {
            Assert.Inconclusive("not implemented yet");
        }

        [TestMethod]
        [ExpectedException(typeof(AssertInconclusiveException))]
        public void ViewScheduleByRoom()
        {
            Assert.Inconclusive("not implemented yet");
        }

        [TestMethod]
        [ExpectedException(typeof(AssertInconclusiveException))]
        public void ViewScheduleByType()
        {
            Assert.Inconclusive("not implemented yet");
        }
        #endregion
         
        */

        // I broke this by changing the code to load ConventionData.xml and contents.xml - options are
        // 1. add these files in the TestData project in the same XML folder location
        // 2. create a better mock schedule that feeds them files
        // for now I've gone with 1
        [TestMethod]
        [Asynchronous]
        [Tag("search")]
        public void ViewScheduleByStars()
        {
            _createAndPopulateSchedule("Tests\\Data\\GoodXML.xml");
            var enumerator = _schedule.Events.GetEnumerator();
            enumerator.MoveNext();
            _event = enumerator.Current;
            Assert.IsNotNull(_event);
            Assert.IsFalse(_event.Star);
            _event.Star = true; //this should add it to 'my schedule'
            _schedule.SaveEvents();

            string pivotString = ScheduleViewModel.PivotView.Stars.ToString();
            MockViewModel = new ScheduleViewModel(null, pivotString, null, true);
            Assert.IsNotNull(MockViewModel);

             // make sure we wait for the ScheduleLoadingComplete to happen            
            MockViewModel.VM_ScheduleLoadingComplete += 
                new EventHandler<ScheduleLoadingEventArgs>(ScheduleCallback);
            MockViewModel.LoadSchedule();
            // wait for our method to trigger on the event and set this to true
            EnqueueConditional(() => _callbackDone);

            foreach (var slice in MockViewModel.EventSlices)
            {
                Assert.IsNotNull(slice);
            }
            var sliceEnumerator = MockViewModel.EventSlices.GetEnumerator();// returns a set of event slices
            // Friday, Saturday, Sunday - it's by day. check the day of our test event

            sliceEnumerator.MoveNext();
            while (!string.Equals(sliceEnumerator.Current.slicePageTitle, _event.day))
            {
                sliceEnumerator.MoveNext();
            }
            var eventSlices = sliceEnumerator.Current;

            var faveEvents = eventSlices.events.GetEnumerator(); // returns a set of events
            faveEvents.MoveNext();
            Event faveEvent = faveEvents.Current;
            Assert.IsNotNull(faveEvent);

            Assert.AreEqual(faveEvent.Location, _event.Location);
            Assert.AreEqual(faveEvent.Kind, _event.Kind);
            Assert.AreEqual(faveEvent.Name, _event.Name);
            Assert.AreEqual(faveEvent.Star, true);
            Assert.AreEqual(faveEvent.friendlyStartTime, _event.friendlyStartTime);


            TestComplete();

        }

        //event handler helper - triggered by the ScheduleLoading event in the ScheduleViewModel, sets _callbackDone to true
        public void ScheduleCallback(object sender, ScheduleLoadingEventArgs e)
        {
            _callbackDone = true;
        }



        [TestMethod]
        [Asynchronous]
        [Tag("search")]
        public void VerifySimpleSearch()
        {
            _createAndPopulateSchedule("Tests\\Data\\GoodXML.xml");
            /* using the search string 'first' we expect to find only this event:
		        <Event
		         kind ="Contest"
		        datetime="08/26/2011 12:00:00"
		        end ="03/11/2011 14:00:00"
		        location="Console"
		        name="First Test Event"
		        description="event 1"
                />
             */

            string pivotType = ScheduleViewModel.PivotView.Search.ToString();
            string searchValue = "First";
            //create viewmodel that will contain the schedule search results
            MockViewModel = new ScheduleViewModel(null, pivotType, searchValue, true);
            Assert.IsNotNull(MockViewModel);

            // set up to wait for the ScheduleLoadingComplete to happen       
            MockViewModel.VM_ScheduleLoadingComplete +=
                new EventHandler<ScheduleLoadingEventArgs>(ScheduleCallback);
            // wait for our method to trigger on the event and set this to true
            EnqueueConditional(() => _callbackDone);
            MockViewModel.LoadSchedule();

            //read the returned scheduleslice and verify it matches the event we expected
            Assert.IsNotNull(MockViewModel.EventSlices);
            foreach (var slice in MockViewModel.EventSlices)
            {
                Assert.IsNotNull(slice);
            }
            var sliceEnumerator = MockViewModel.EventSlices.GetEnumerator();// returns a set of event slices
            sliceEnumerator.MoveNext(); //our target event is on friday, the first slice
            var eventSlices = sliceEnumerator.Current;
            var resultEvents = eventSlices.events.GetEnumerator(); // returns a set of events
            resultEvents.MoveNext();
            var resultEvent = resultEvents.Current;

            Assert.IsNotNull(resultEvent);

            Assert.AreEqual(resultEvent.Location, "Console");
            Assert.AreEqual(resultEvent.Kind, "Contest");
            Assert.AreEqual(resultEvent.Name, "First Test Event");
            Assert.AreEqual(resultEvent.Star, false);
            Assert.AreEqual(resultEvent.Details, "event 1");

            _callbackDone = false;
            TestComplete();
        }

        
        [TestMethod]
        [Asynchronous]
        [Tag("search")]
        public void VerifyEmptySearchResult()
        {
            _createAndPopulateSchedule("\\Tests\\Data\\GoodXML.xml");
            string pivotType = ScheduleViewModel.PivotView.Search.ToString();
            string searchValue = "aslkdfhahsg"; //no results for this
            //create viewmodel that will contain the schedule search results
            MockViewModel = new ScheduleViewModel(null, pivotType, searchValue, true);
            Assert.IsNotNull(MockViewModel);

            // set up to wait for the ScheduleLoadingComplete to happen       
            MockViewModel.VM_ScheduleLoadingComplete +=
                new EventHandler<ScheduleLoadingEventArgs>(ScheduleCallback);
            // wait for our method to trigger on the event and set this to true
            EnqueueConditional(() => _callbackDone);
            MockViewModel.LoadSchedule();

            //read the returned scheduleslice and verify it matches the event we expected
            Assert.IsNotNull(MockViewModel.EventSlices);
            foreach (var slice in MockViewModel.EventSlices)
            {
                Assert.IsNotNull(slice);
            }
            var sliceEnumerator = MockViewModel.EventSlices.GetEnumerator();// returns a set of event slices
            sliceEnumerator.MoveNext(); //our target event is on friday, the first slice
            var eventSlices = sliceEnumerator.Current;
            var resultEvents = eventSlices.events.GetEnumerator(); // returns a set of events
            resultEvents.MoveNext();
            var resultEvent = resultEvents.Current;

            Assert.IsNull(resultEvent);


            _callbackDone = false;
            TestComplete();
        }


      
    }
}
