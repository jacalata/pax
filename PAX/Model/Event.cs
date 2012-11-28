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
using System.ComponentModel;
using Coding4Fun.Phone.Controls;

namespace PAX7.Model
{

    public class MenuOption
    {
        public MenuOption(string title, string URI)
        {
            Title = title;
            Destination = URI;
        }
        //title to display
        public string Title { get; set;  }

        //location to go to
        public string Destination { get; set; }

    }

    public class Event : INotifyPropertyChanged
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
        public string UserNotes
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
                return day +" " + time;
            }
        }

        // convenience method to return the start time in a readable string
        public string time
        {
            get
            {
                return StartTime.ToShortTimeString();
            }
        }

        // convenience method to return the day in a readable string
        public string day
        {
            get
            {
                return StartTime.DayOfWeek.ToString();
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
        public float hoursDuration
        {
            get
            {
                TimeSpan timespan = EndTime - StartTime;
                return timespan.Hours + (timespan.Minutes / 60);
            }/*
            set
            {
                int hours = int.Parse(value.ToString());
                int minutes = (value - hours) * 100;
                EndTime = StartTime + new DateTime((int)value, ;
            }
              */
        }

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
                RaisePropertyChanged("Star");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void RaisePropertyChanged(string propertyName)
        {
            if (this.PropertyChanged != null)
                this.PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            

        }

        //Display the details of this event in a message box and expose the option to add/remove from starred group
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

    }



}
