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
        public void _createAndPopulateSchedule(string testDataFile)
        {
            // populate the isolated storage
            _schedule.NukeAllStorage();
            List<string> filenames = new List<string>();
            filenames.Add(testDataFile);
            _schedule.GetXMLEvents(filenames, true); //isFirstRun
            Assert.IsNotNull(_schedule.Events, "event collection");
            var _emptyEvents = new ObservableCollection<Event>();
            Assert.AreNotEqual(_schedule.Events, _emptyEvents);
            _schedule.SaveEvents(); // to isolated storage
        }

        [TestCleanup]
        public void _clearSchedule()
        {
            _schedule.Clear();
        }

        /*
        [TestMethod]
        [Tag("PostUpdate")]
        [ExpectedException(typeof(AssertInconclusiveException))]
        public void VerifyPersonalScheduleAfterUpdate()
        {
            //need to have stored a copy of the data before updating, then compare to it 
            Assert.Inconclusive("not implemented yet");
        }

        [TestMethod]
        [Tag("PreUpdate")]
        [ExpectedException(typeof(AssertInconclusiveException))]
        public void SavePersonalScheduleBeforeUpdate()
        {
            _schedule.SaveEvents();
            Assert.Inconclusive("not implemented yet");
            //validate isolated storage here
        }
         */


    }
}
