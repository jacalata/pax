using System;

namespace PAX7
{

    //I don't even remember why this was created or if it's being used
    public class StateUtilities
    {
        private static Boolean isLaunching;

        public static Boolean IsLaunching
        {
            get { return isLaunching; }
            set { isLaunching = value; }
        }

    }
}
