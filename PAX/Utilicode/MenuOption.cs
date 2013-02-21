using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace PAX7.Utilicode
{
    class MenuOption
    {
            public MenuOption(string title, string URI)
            {
                Title = title;
                Destination = URI;
            }
            //title to display
            public string Title { get; set; }

            //location to go to
            public string Destination { get; set; }

       
    }
}
