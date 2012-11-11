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
    public class SearchTests : SilverlightTest
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

        // enter a search query, confirm that the results match what we expect
    }
}
