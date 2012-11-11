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
using PAX7.View; //uhoh
using PAX7.ViewModel;
using PAX7.Tests.Fakes;

using System.Collections.Generic; //list<string>
using System.Collections.ObjectModel; //observablecollection

namespace PAX7.Tests
{
    [TestClass]
    public class ScheduleTests : SilverlightTest
    {
        
        private Schedule _schedule = new Schedule();
        private ScheduleViewModel _viewModel;
        private Event _event;
        private bool _callbackDone = false;


        [TestInitialize]
        public void _createAndPopulateSchedule()
        {
            // populate the isolated storage
            _schedule.NukeAllStorage();
            List<string> filenames = new List<string>();
            filenames.Add("Tests\\Data\\GoodXML.xml");
            _schedule.GetXMLEvents(filenames, true); //isFirstRun
            Assert.IsNotNull(_schedule.Events, "event collection");
            var _emptyEvents = new ObservableCollection<Event>();
            Assert.AreNotEqual(_schedule.Events, _emptyEvents);
            _schedule.SaveEvents(); // to isolated storage
        }

        [TestMethod]
        [Asynchronous]
        public void ViewScheduleByStars()
        {
            _viewModel = null; //just making sure
            var enumerator = _schedule.Events.GetEnumerator();
            enumerator.MoveNext();
            _event = enumerator.Current;
            Assert.IsNotNull(_event);
            Assert.IsFalse(_event.Star);
            _event.Star = true; //this should add it to 'my schedule'
            _schedule.SaveEvents();

            SchedulePivotView pivotView = new SchedulePivotView(); //haven't implemented a fake one of this yet
            string pivotParam = ScheduleViewModel.PivotView.Stars.ToString(); // pivotView.pivotString;
            _viewModel = new ScheduleViewModel(pivotView, pivotParam);
            Assert.IsNotNull(_viewModel);

             // make sure we wait for the ScheduleLoadingComplete to happen            
            _viewModel.VM_ScheduleLoadingComplete += 
                new EventHandler<ScheduleLoadingEventArgs>(ScheduleCallback);
            _viewModel.LoadSchedule();
            // wait for our method to trigger on the event and set this to true
            EnqueueConditional(() => _callbackDone);

            foreach (var slice in _viewModel.EventSlices)
            {
                Assert.IsNotNull(slice);
            }
            var sliceEnumerator = _viewModel.EventSlices.GetEnumerator();// returns a set of event slices
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

            //cleanup
            _viewModel = null;
            _event = null; 
            _callbackDone = false;
            _schedule.Clear();
            TestComplete();
        }

        //event handler helper - triggered by the ScheduleLoading event in the ScheduleViewModel, sets _callbackDone to true
        public void ScheduleCallback(object sender, ScheduleLoadingEventArgs e)
        {
            _callbackDone = true;
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
            _schedule.SaveEvents();
            //validate isolated storage here
        }


        [TestMethod]
        [TestProperty("TestCategory", "Constructors")]
        public void CreateSchedulePivotView()
        {
            Assert.Inconclusive("not implemented yet");
        }

        #region UI Tests?
        [TestMethod]
        public void ViewScheduleByDay()
        {
            Assert.Inconclusive("not implemented yet");
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

        #endregion
         
    }
}
