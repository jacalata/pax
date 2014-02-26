using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using Coding4Fun.Phone.Controls;
using PAX7.Model;

namespace PAX7.ViewModel
{
    class EventView
    {    
        // Display the details of this event in a message box 
        // and expose the option to add/remove from starred group
        public static void ShowEventDetails(Event e)
        {
            AboutPrompt detailsPopup = new AboutPrompt();
            detailsPopup.Title = "";// this.Name; //what to do about long names????

            // a side scroller!!!
            detailsPopup.VersionNumber = "";
            ContentControl detailsBody = new ContentControl();
            detailsBody.DataContext = e;
            detailsBody.Template = App.Current.Resources["aboutEvent"] as ControlTemplate;
            detailsPopup.Body = detailsBody;
            detailsPopup.Show();
        }
    }
}
