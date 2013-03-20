using System;
using System.ComponentModel;
using System.Windows.Controls;
using Coding4Fun.Phone.Controls;
using Microsoft.Phone.Scheduler; //for the reminders. Pretty sure they shouldn't be in this class? where do they fit in mvvm?
using PAX7.Utilicode; //settings

namespace PAX7.Model
{

    public class Event : INotifyPropertyChanged, IComparable
    {
        // constructor - does nothing, no default values. 
        public Event() {}

        //session title
        private string _name;
        public string Name 
        {
            get
            {
                return _name;
            }
            set
            {
                _name = value;
                RaisePropertyChanged("Name");
            }
        }

        // potentially long description
        private string _details;
        public string Details
        {
            get
            {
                return _details;
            }
            set
            {
                _details = value;
                RaisePropertyChanged("Details");
            }
        }

        // user editable notes on the event. 
        private string _usernotes;
        internal string UserNotes
        {
            get
            {
                return _usernotes;
            }
            set
            {
                _usernotes = value;
                RaisePropertyChanged("UserNotes");
            }
        }

        //kind of event: panel, speech, omegathon, contest, freeplay, music
        private string _kind;
        public string Kind
        {
            get
            {
                return _kind;
            }
            set
            {
                _kind = value;
                RaisePropertyChanged("Kind");
            }
        }

        #region time handling
        // start time 
        private DateTime _starttime;
        public DateTime StartTime
        {
            get
            {
                return _starttime;
            }
            set
            {
                _starttime = value;
                RaisePropertyChanged("StartTime");
            }
        }

        // convenience method to return the start time and day in a readable string
        public string friendlyStartTime
        {
            get
            {
                return friendlyDate + " " + time;
            }
        }

        // convenience method to return the start time in a readable string
        internal string time
        {
            get
            {
                return StartTime.ToShortTimeString();
            }
        }

        //convenience method to return the day of week
        public string day
        {
            get
            {
                return StartTime.DayOfWeek.ToString();
            }
        }


        // convenience method to return the day and date in a readable string
        internal string friendlyDate
        {
            get
            {
                return StartTime.DayOfWeek.ToString() + " " + StartTime.Date.ToString("d");
            }
        }

        // end time
        private DateTime _endtime;
        public DateTime EndTime
        {
            get
            {
                return _endtime;
            }
            set
            {
                _endtime = value;
                RaisePropertyChanged("EndTime");
            }
        }

        // convenience method to return the duration of the event
        internal float hoursDuration
        {
            get
            {
                TimeSpan timespan = EndTime - StartTime;
                return timespan.Hours + (timespan.Minutes / 60);
            }/* could ask for this is we enable user-entered events
            set
            {
                int hours = int.Parse(value.ToString());
                int minutes = (value - hours) * 100;
                EndTime = StartTime + new DateTime((int)value, ;
            }
              */
        }

        #endregion
        
        // location
        private string _location;
        public string Location
        {
            get
            {
                return _location;
            }
            set
            {
                _location = value;
                RaisePropertyChanged("Location");
            }
        }

        // has the user starred this event/put it in their schedule
        private bool _starred;  
        public bool Star
        {
            get
            {
                return _starred;
            }
            set
            {
                _starred = value;
                if (_starred) SetReminder();
                else UnsetReminder();
                RaisePropertyChanged("Star");
            }
        }

        #region reminders
        // guid for the reminder name used internally in the scheduler
        private string _reminderName = null;

        /// <summary>
        /// Set a reminder for this event 1 hour before the scheduled start
        /// if scheduled start is past, do not create a reminder unless testing=true
        /// code taken from http://windowsphonegeek.com/articles/getting-started-with-windows-phone-reminders
        /// </summary>
        /// <param name="testing">for testing, make the reminder actually schedule 10 seconds from now</param>
        internal void SetReminder(bool testing=false)
        {
            if (!IsoStoreSettings.IsAllowedSetReminders())
                return;
            // event was starred - create reminder
            _reminderName = Guid.NewGuid().ToString();
            // use guid for reminder name, since reminder names must be unique
            Reminder reminder = new Reminder(_reminderName);
            int MAX_LENGTH = 62; // max length is 63 characters, per msdn
            int titleLength;
            DateTime reminderStartTime;
            try
            {
                // NOTE: setting the Title property is supported for reminders
                // in contrast to alarms where setting the Title property is not supported
                titleLength = this.Name.Length > MAX_LENGTH ? MAX_LENGTH : this.Name.Length;
                reminder.Title = this.Name.Substring(0, titleLength); 
                reminder.Content = "scheduled " + this.Kind + " begins at " + this.friendlyStartTime + " in " + this.Location;

                //NOTE: the value of BeginTime must be after the current time
                //set the BeginTime time property in order to specify when the reminder should be shown
                if (this.StartTime == new DateTime(0))
                {
                    //the event time is set to the start of time, don't want/can't set a reminder for that. 
                    return;
                }
                reminderStartTime = this.StartTime.Subtract(new TimeSpan(1, 0, 0));//start time minus one hour
                if (reminderStartTime < DateTime.Now)
                {
                    if (testing)
                        reminderStartTime = DateTime.Now + new TimeSpan(0, 0, 30);
                    else
                        return; //don't create reminders 'now' for real usage
                }
                reminder.BeginTime = reminderStartTime; 
                // NOTE: ExpirationTime must be after BeginTime
                reminder.ExpirationTime = reminderStartTime + new TimeSpan(1,0,0); 
            }
            catch (Exception e)
            {
                // gets called during app initialisation with some weird issues
                LittleWatson.ReportException(e, "time exceptions creating reminder for event " + this.Name);
                return;
            }

            // NOTE: another difference from alerts
            // you can set a navigation uri that is passed to the application when it is launched from the reminder
            //reminder.NavigationUri = new Uri("/View/SchedulePivotView.xaml?PivotOn=Stars");
            if (ScheduledActionService.Find(_reminderName) != null)
                ScheduledActionService.Remove(_reminderName);
            ScheduledActionService.Add(reminder);
        }

        /// <summary>
        /// Remove the scheduled reminder for this event
        /// Possible the reminder does not actually exist, if the user has been changing their preferences on adding a reminder
        /// </summary>
        internal void UnsetReminder()
        {
            if (_reminderName != null && ScheduledActionService.Find(_reminderName) != null )
                ScheduledActionService.Remove(_reminderName);
            _reminderName = null;
        }

        #endregion reminders

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
        }

        //Display the details of this event in a message box and expose the option to add/remove from starred group
        //TODO this should be in view code somewhere
        public void ShowEventDetails()
        {

            AboutPrompt detailsPopup = new AboutPrompt();
            detailsPopup.Title = "";// this.Name; //what to do about long names????
           
            // a side scroller!!!
            detailsPopup.VersionNumber = "";
            ContentControl detailsBody = new ContentControl();
            detailsBody.DataContext = this;
            detailsBody.Template = App.Current.Resources["aboutEvent"] as ControlTemplate;
            detailsPopup.Body = detailsBody;
            detailsPopup.Show();
        }
    

        // Create a copy of an event to save to isolated storage
        // If your object is databound, this copy is not databound.
        public Event GetCopy()
        {
            Event copy = (Event)this.MemberwiseClone();
            return copy;
        }

        /// <summary>
        /// implement IComparable to enable List.Sort
        /// </summary>
        /// <param name="other">another Event object to compare against</param>
        /// <returns></returns>
        public int CompareTo(object other)
        {
            Event otherEvent = (Event)other;
            if (this.StartTime == otherEvent.StartTime)
                return this.Name.CompareTo(otherEvent.Name);
            else
                return this.StartTime.CompareTo(otherEvent.StartTime);
        }


        /// <summary>
        /// test method to generate fake events
        /// </summary>
        internal void populate()
        {
            Name = "fakeName";
            Details = "this is a fake event";
            Kind = "Panel";
            StartTime = new DateTime(2000, 2, 2, 10, 0, 0);
            EndTime = new DateTime(2000, 2, 2, 11, 0, 0);
            Location = "Room1";
            Star = false;
        }
    }



}
