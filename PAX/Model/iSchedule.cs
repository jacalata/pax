using System;
using System.Collections.Generic;
using System.Collections.ObjectModel; //observablecollection
using System.Linq;
using System.Text;

namespace PAX7.Model
{
    public abstract class iSchedule
    {

        protected void RaiseScheduleLoadingEvent(ObservableCollection<Event> Events)
        {
            ScheduleLoadingComplete(this,
              new ScheduleLoadingEventArgs(Events));
        }

        /// <summary>
        /// thrown at the end of GetEvents for the ViewModel
        /// </summary>
        public event EventHandler<ScheduleLoadingEventArgs> ScheduleLoadingComplete;

        public List<string> eventLocations;
        public List<string> eventTypes;
        public List<string> eventDays;

        public abstract void GetEvents();

        public abstract void checkForNewSchedule(string uri);
        public abstract void DownloadNewEventFiles(string uri);


        /// <summary>
        /// event triggered during webClient_VersionInfoCompleted, waited on by the ViewModel
        /// </summary>
        public EventHandler evt_updateCheckComplete;

        /// <summary>
        /// event triggered by webClient_OpenReadCompleted for the viewmodel
        /// </summary>
        public EventHandler evt_downloadScheduleComplete;



    }
}
