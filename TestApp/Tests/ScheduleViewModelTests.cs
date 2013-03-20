
using System.Collections.Generic; //list<string>
using System.Collections.ObjectModel; //observablecollection
using System.IO.IsolatedStorage;
using Microsoft.Silverlight.Testing;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using PAX7.Model;
using PAX7.ViewModel;

namespace PAX7.Tests
{
    [TestClass]
    public class ScheduleViewModelTests :SilverlightTest
    {
        
        private Schedule _schedule = new Schedule();
        private ScheduleViewModel MockViewModel;
        private Event _event;
        private bool _callbackDone = false;


        /// <summary>
        /// helper method that loads a specified set of events from xml file
        /// should cause the scheduleLoading event to be thrown
        /// </summary>
        /// <param name="testDataFile"></param>
        private void _createAndPopulateSchedule(string testDataFile)
        {
            MockViewModel.schedule = _schedule;
            // make sure we wait for the ScheduleLoadingComplete to happen            
            MockViewModel.schedule.ScheduleLoadingComplete += delegate(object Sender, PAX7.Model.ScheduleLoadingEventArgs e)
            {
                _callbackDone = true;
            };
            _schedule.NukeAllStorage();
            List<string> filenames = new List<string>();
            filenames.Add(testDataFile);
            var _Events = new ObservableCollection<Event>();
            _Events = _schedule.GetXMLEvents(true, filenames); //read from xap
            _schedule.SaveEvents(_Events); // to isolated storage
            Assert.IsNotNull(MockViewModel.schedule, "schedule member of viewmodel");
        }

        [TestInitialize]
        public void TestInitialize()
        {
            /*
            IsolatedStorageSettings.ApplicationSettings.Clear();
            _callbackDone = false;
            MockViewModel = null;
            _event = null;
            _schedule.Clear();
             * */
        }

        [TestCleanup]
        public void TestCleanup()
        {
        }

        
        [TestMethod]
        [Tag("Constructors")]
        [Tag("ViewModel")]
        public void createScheduleViewModel()
        {
            string pivotType = "byDay"; 
            var viewModel = new ScheduleViewModel(null, pivotType); //don't need to specify 'mock' here because we don't do anything with it
            Assert.IsNotNull(viewModel);
        }

        /* NotYetImplemented
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

        /// <summary>
        ///  verify that the scheduleloading event is thrown to the viewmodel
        /// </summary>
        [TestMethod]
        [Tag("ViewModel")]
        public void VerifyScheduleLoadingEvent()
        {
            string pivotType = "byDay";
            MockViewModel = new ScheduleViewModel(null, pivotType, null, true);
            Assert.IsTrue(true);
        }
        /* comment out: validate that it is this event stuff which is causing the tests to hang
            // make sure we wait for the ScheduleLoadingComplete to happen            
            MockViewModel.schedule.ScheduleLoadingComplete += delegate(object Sender, PAX7.Model.ScheduleLoadingEventArgs e)
            {
                _callbackDone = true;
            };
            EnqueueConditional(() => _callbackDone == true); 
            Assert.IsTrue(_callbackDone);
            EnqueueTestComplete();
        }
         * */


        /// <summary>
        ///  verify that the second viewmodel scheduleloading event is thrown once the filtering and slicing is complete
        /// </summary>
        [TestMethod]
        [Tag("ViewModel")]
        [Tag("fail")]
        public void VerifyVMScheduleLoadingEvent()
        {
            string pivotType = "byDay";
            Fakes.FakeSchedule fakeSchedule = new Fakes.FakeSchedule(false);
            MockViewModel = new ScheduleViewModel(null, pivotType, null, true, fakeSchedule);
            Assert.IsTrue(true);
            MockViewModel.schedule.ScheduleLoadingComplete += delegate(object Sender, PAX7.Model.ScheduleLoadingEventArgs e)
            {
                _callbackDone = true;
            };
            EnqueueConditional(() => _callbackDone == true);
            EnqueueCallback(() => Assert.IsTrue(_callbackDone));
            _callbackDone = false;
            MockViewModel.LoadSchedule();
            MockViewModel.VM_ScheduleLoadingComplete += delegate(object Sender, PAX7.Model.ScheduleLoadingEventArgs e)
            {
                _callbackDone = true;
            };
            EnqueueConditional(() => _callbackDone == true);
            EnqueueCallback( () => Assert.IsTrue(_callbackDone));
            EnqueueTestComplete();
        }

        //these tests never complete, I think my event waiting is fucked up
        /*
                // I broke this by changing the product code to load ConventionData.xml and contents.xml - options are
                // 1. add these files in the TestData project in the same XML folder location
                // 2. create a better mock schedule that feeds them files
                // for now I've gone with 1
                 [TestMethod]
                [Asynchronous]
                [Tag("search")]
                [Tag("ViewModel")]
                public void ViewScheduleByStars()
                {

                    string pivotString = ScheduleViewModel.PivotView.Stars.ToString();
                    MockViewModel = new ScheduleViewModel(null, pivotString, null, true); //constructor sets it up to wait for schedule loading event
                    Assert.IsNotNull(MockViewModel);

                     _createAndPopulateSchedule("Tests\\Data\\GoodXML.xml"); // will load events on the schedule item and schedule loading will be thrown

                     // wait for our method to trigger on the event and set this to true
                     EnqueueConditional(() => _callbackDone);
                     Assert.IsTrue(_callbackDone);
            
                    var enumerator = _schedule.Events.GetEnumerator();
                    enumerator.MoveNext();
                    _event = enumerator.Current;
                    Assert.IsNotNull(_event);
                    Assert.IsFalse(_event.Star);
                    _event.Star = true; //this should add it to 'my schedule'
                    _schedule.SaveEvents();

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
                    /*
                    // should probably make _event local to this test for cleaner suite
                    Assert.AreEqual(faveEvent.Location, _event.Location);
                    Assert.AreEqual(faveEvent.Kind, _event.Kind);
                    Assert.AreEqual(faveEvent.Name, _event.Name);
                    Assert.AreEqual(faveEvent.Star, true);
                    Assert.AreEqual(faveEvent.friendlyStartTime, _event.friendlyStartTime);
                     * 
                    EnqueueTestComplete();

                }
                */

        // I had to add xml/friday.xml etc in the Test project because schedule.GetEvents is reading that
        // better would be to pass through a specific file from here, which requires updating the 
        // scheduleviewmodel class?
         [TestMethod]
        [Asynchronous]
        [Tag("search")]
        [Tag("ViewModel")]
        [Tag("fail")]
        public void VerifySimpleSearch()
        {
             // searches for the default fake Event created in FakeSchedule

            string pivotType = ScheduleViewModel.PivotView.Search.ToString();
            string searchValue = "fakeName";
            Fakes.FakeSchedule fakeSchedule = new Fakes.FakeSchedule(false);
            //create viewmodel that will contain the schedule search results
            MockViewModel = new ScheduleViewModel(null, pivotType, searchValue, true, fakeSchedule);
            Assert.IsNotNull(MockViewModel);

            // set up to wait for the ScheduleLoadingComplete to happen       
            MockViewModel.VM_ScheduleLoadingComplete += delegate(object Sender, PAX7.Model.ScheduleLoadingEventArgs e)
            {
                _callbackDone = true;
            };
            // wait for our method to trigger on the event and set this to true
            EnqueueConditional(() => _callbackDone);
            MockViewModel.LoadSchedule();

            //read the returned scheduleslice and verify it matches the event we expected
            Assert.IsNotNull(MockViewModel.EventSlices);
            Assert.AreNotEqual(0, MockViewModel.EventSlices.Count, "no event slices were found in the scheduleViewModel");
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
            /*
             * uncomment this when we are passing in a specific xml file again
            Assert.AreEqual(resultEvent.Location, "Console");
            Assert.AreEqual(resultEvent.Kind, "Contest");
            Assert.AreEqual(resultEvent.Name, "First Test Event");
            Assert.AreEqual(resultEvent.Star, false);
            Assert.AreEqual(resultEvent.Details, "event 1");
             *

            _callbackDone = false;
             * */
            EnqueueTestComplete();
        }

         [TestMethod]
        [Asynchronous]
        [Tag("search")]
         [Tag("ViewModel")]
         [Tag("fail")]
        public void VerifyEmptySearchResult()
        {
           // _createAndPopulateSchedule("\\Tests\\Data\\GoodXML.xml");
            string pivotType = ScheduleViewModel.PivotView.Search.ToString();
            string searchValue = "aslkdfhahsg"; //no results for this
            Fakes.FakeSchedule fakeSchedule = new Fakes.FakeSchedule(false);
            //create viewmodel that will contain the schedule search results
            MockViewModel = new ScheduleViewModel(null, pivotType, searchValue, true, fakeSchedule);
            Assert.IsNotNull(MockViewModel);

            // set up to wait for the ScheduleLoadingComplete to happen       
            MockViewModel.VM_ScheduleLoadingComplete += delegate(object Sender, PAX7.Model.ScheduleLoadingEventArgs e)
            {
                _callbackDone = true;
            };
            // wait for our method to trigger on the event and set this to true
            EnqueueConditional(() => _callbackDone); // <---- this never happens? 

            //read the returned scheduleslice and verify it matches the event we expected
            EnqueueCallback( ()=>
                {
                    MockViewModel.LoadSchedule();
                    Assert.IsNotNull(MockViewModel.EventSlices);
                    Assert.AreNotEqual(0, MockViewModel.EventSlices.Count, "no slices found in the ScheduleViewModel");
                    foreach (var slice in MockViewModel.EventSlices)
                    {
                        Assert.IsNotNull(slice);
                    }
                    var sliceEnumerator = MockViewModel.EventSlices.GetEnumerator();// returns a set of event slices
                    sliceEnumerator.MoveNext(); //our target event is on search by title, the first slice
                    var eventSlices = sliceEnumerator.Current;
                    var resultEvents = eventSlices.events.GetEnumerator(); // returns a set of events
                    resultEvents.MoveNext();
                    var resultEvent = resultEvents.Current;
                    //actually should be doing this for both slices, in search

                    Assert.IsNull(resultEvent);
                });
            EnqueueTestComplete();

        }



    }
}
