using System;
using System.Collections.Generic; //IEnumerable
using System.Collections.ObjectModel;
using System.Linq;
using PAX7.Model;
using PAX7.View;
using Telerik.Windows.Data;//?

namespace PAX7.ViewModel
{

    public class ScheduleSlice
    {

        public List<Event> events { get; set; }
        public List<DataDescriptor> eventSortField { get; set; }
        public string slicePageTitle{ get; set; }
        // note that says how to sort events into this slice, to make all the pivot code more generic
        // haven't figured it out yet
        // IEnumerable<Event> query; //= events.Where((evt, day) => day.equals(Friday));

        public ScheduleSlice(string title, List<Event> listEvents)
        {
            events = new List<Event>();
            events.AddRange(listEvents);
            events.Sort();
            slicePageTitle = title;
        }
    }

    public class ScheduleViewModel
    {
        public ObservableCollection<ScheduleSlice> EventSlices;
        public enum PivotView { Day, EventType, Location, Stars, Search, Expo }; 
        public enum SearchFields {Title, Description };
        public string pivotTemplateName; //choose between jumplist and listbox for different views of events

        internal bool mocking = false; //if true, skip ui callbacks because we are running in a test
        internal Schedule schedule;
        
        private SchedulePivotView view;
        private string pivot;
        private ObservableCollection<Event> events;
        private string searchQuery;


        #region Constructors
        /// <summary>
        /// Construct a view of events filtered according to the pivotParameter
        /// </summary>
        /// <param name="view">hook to the view UI so we can make it display the events</param>
        /// <param name="pivotParameter">enum telling us which view the user has selected</param>
        /// <param name="searchQuery">optional parameter that only applies if we are in the search pivot</param>
        public ScheduleViewModel(SchedulePivotView view, string pivotParameter, string searchQuery = null, bool mock = false)
        {
            this.schedule = new Schedule();
            this.events = new ObservableCollection<Event>();
            this.EventSlices = new ObservableCollection<ScheduleSlice>();
            this.view = view;
            this.schedule.ScheduleLoadingComplete +=
                new EventHandler<ScheduleLoadingEventArgs>(Schedule_ScheduleLoadingComplete);
            pivot = pivotParameter;
            pivotTemplateName = "pivotJumplistTemplate";//default
            this.searchQuery = searchQuery;
            this.mocking = mock;
        }
        #endregion
                
        public void LoadSchedule()
        {
            schedule.GetEvents();
        }

        /// <summary>
        /// This is fired by the Schedule_ScheduleLoadingComplete method below
        /// </summary>
        public event EventHandler<ScheduleLoadingEventArgs> VM_ScheduleLoadingComplete;

        /// <summary>
        /// This method is set in the constructor to be triggered by the ScheduleLoadingComplete event 
        /// which is thrown when the Schedule member finishes loading. 
        /// It calls the relevant 'filter' method to create the event slices for viewing
        /// It will then call the UI dispatcher to display the events. s
        /// It will also throw its own event for non-UI tests to react to. 
        /// </summary>
        internal void Schedule_ScheduleLoadingComplete(object sender, ScheduleLoadingEventArgs e)
        {
            // Add the new Events            
            foreach (Event evt in e.Results)
                events.Add(evt);
            //filter according to current pivot  
            if (pivot.Equals((PivotView.EventType).ToString()))
                filterEventsByEventType();
            else if (pivot.Equals((PivotView.Location).ToString()))
                filterEventsByLocation();
            else if (pivot.Equals((PivotView.Stars).ToString()))
                filterEventsByStars();
            else if (pivot.Equals((PivotView.Search).ToString()))
                filterEventsBySearch();
            else if (pivot.Equals((PivotView.Expo).ToString()))
                filterEventsToExpo();
            else // default is by day (pivot.Equals(PivotView.Day.ToString())  )
                filterEventsByDay();
 
            if (mocking)
            {
                // Fire event for test code to know we're all done
                if (VM_ScheduleLoadingComplete != null)
                {
                    VM_ScheduleLoadingComplete(this, e);
                }
            }
            else
            {
                // Fire Event on UI Thread
                view.Dispatcher.BeginInvoke(() =>
                {
                    view.OnLoadComplete();
                });
            }
        }

        // create a single loooong list of expo booths
        void filterEventsToExpo()
        {
            var eventsVar =
           (from Event in events
            where Event.Kind.Equals("Expo")
            select Event).ToList<Event>();
            ScheduleSlice expoList = new ScheduleSlice("all exhibitors", eventsVar);
            EventSlices.Add(expoList);

        }


        /// <summary>
        /// create the slices, retrieving events that contain the given search term in either title or description,
        /// and dividing events based on which field it was found in
        /// </summary>
       void filterEventsBySearch()
        {
              var eventsVar =
             (from Event in events
              where isContainedIn(Event.Name, searchQuery) 
              select Event).ToList<Event>();
             ScheduleSlice eventTitlesSlice = new ScheduleSlice("title", eventsVar);
             EventSlices.Add(eventTitlesSlice);

             eventsVar.Clear();
              eventsVar =
             (from Event in events
              where isContainedIn(Event.Details, searchQuery)
              select Event).ToList<Event>();
              ScheduleSlice eventDetailsSlice = new ScheduleSlice("description", eventsVar);
              EventSlices.Add(eventDetailsSlice);
            
        }

        /// <summary>
        /// helper method for searching - does a case insensitive string 'contains'
        /// needed to build this method to fit in the linq query syntax
        /// </summary>
        /// <param name="searchField">the event data</param>
        /// <param name="searchTerm">the string we are looking for</param>
        /// <returns></returns>
       private static bool isContainedIn(string searchField, string searchTerm)
        {
            return searchField.ToUpperInvariant().Contains(searchTerm.ToUpperInvariant()); 
        }

        /// <summary>
        /// create the slices, dividing events based on the event day
        /// </summary>
       void filterEventsByDay()
        {
            List<string> eventDays = schedule.eventDays;
            ScheduleSlice daySlice;
            var eventsVar = (List<Event>)null;
            foreach (string dayName in eventDays)
            {
                eventsVar =
                (from Event in events
                 where Event.day.Equals(dayName) &&!Event.Kind.Equals("Expo")
                 select Event).ToList<Event>();
                daySlice = new ScheduleSlice(dayName, eventsVar);
                EventSlices.Add(daySlice);
            }
        }

       /// <summary>
       /// create the slices, dividing events based on the event type (panel, show, etc)
        /// </summary>
       void filterEventsByEventType()
       {
           pivotTemplateName = "pivotListTemplate";

           List<String> eventTypes = schedule.eventTypes;          
           var eventsVar = (List<Event>)null;
           ScheduleSlice eventTypeSlice;
           foreach (string typeName in eventTypes)
           {
               eventsVar =
            (from Event in events
             where Event.Kind == typeName && !Event.Kind.Equals("Expo")
             select Event).ToList<Event>();
               eventTypeSlice = new ScheduleSlice(typeName, eventsVar);
               if (eventsVar.Count != 0)
               {
                   EventSlices.Add(eventTypeSlice);
               }
           }
       }

       /// <summary>
       /// create the slices, dividing events based on the event location
        /// </summary>
       void filterEventsByLocation()
       {
           pivotTemplateName = "pivotListTemplate";

           List<String> eventLocations = schedule.eventLocations;          
           var eventsVar = (List<Event>)null;
           ScheduleSlice eventLocationSlice;
           foreach (string location in eventLocations)
           {
               eventsVar =
            (from Event in events
             where Event.Location.Contains(location) && !Event.Kind.Equals("Expo")
             select Event).ToList<Event>();
               eventLocationSlice = new ScheduleSlice(location, eventsVar);
               if (eventsVar.Count != 0)
               {
                   EventSlices.Add(eventLocationSlice);
               }
           }

       }

       /// <summary>
       /// create the slices, selecting only starred events and dividing them based on the event day
        /// </summary>
       void filterEventsByStars()
       {  
           
            List<string> eventDays = schedule.eventDays;
            ScheduleSlice starSlice;
            var eventsVar = (List<Event>)null;
            foreach (string dayName in eventDays)
            {
                eventsVar =
           (from Event in events
            where Event.day.Equals(dayName) && (Event.Star == true)
            select Event).ToList<Event>();
                starSlice = new ScheduleSlice(dayName, eventsVar);
                EventSlices.Add(starSlice);
            }
       }

        
    }
}
