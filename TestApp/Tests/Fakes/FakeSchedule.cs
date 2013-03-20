using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Collections.ObjectModel; //observablecollection

using PAX7.Model;

namespace PAX7.Tests.Fakes
{
    public class FakeSchedule: iSchedule
    {        
        public FakeSchedule(bool runFromXap)
        {
            eventDays = new List<string>();
            eventDays.Add("Friday");
            eventLocations = new List<string>();
            eventLocations.Add("Room1");
            eventTypes = new List<string>();
            eventTypes.Add("Panel");
        }

        public override void GetEvents()
        {
            ObservableCollection<Event> events = new ObservableCollection<Event>();
            Event fakeEvent = new Event();
            fakeEvent.populate();
            events.Add(fakeEvent);
        }

        // can I literally just leave these empty?
        public override void checkForNewSchedule(string uri)
        {
        }

        public override void DownloadNewEventFiles(string uri)
        {
        }

    }
}
