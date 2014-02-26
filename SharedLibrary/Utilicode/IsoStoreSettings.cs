using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.IsolatedStorage;

namespace PAX7.Utilicode
{
    public static class IsoStoreSettings
    {

        internal static string IsoStoreLastUpdatedRecord = "lastUpdated"; // the name for the value we look at to check the last schedule update date
        internal static string IsoStoreScheduleCreationDate = "scheduleCreationDate"; // the name for the value we look at to check the creation date of the current info
        internal static string IsoStoreHasUpdateAvailable = "hasUpdateAvailable";// the name for the value we look at to check if a new update is available
        internal static string IsoStoreAllowAutoUpdateCheck = "allowAutoUpdating"; // the name for the value to look at to see if we should check for updates 
        internal static string IsoStoreUpdateCheckFailed = "updateCheckFailed"; // the name for the value to look at to see if the update check failed 
        internal static string IsoStoreAllowSetReminders = "allowSetReminders"; // the key for the value of whether the user wants reminders
        internal static string IsoStoreScheduleVersionNumber = "scheduleVersionNumber"; //key for storing the version number of our current schedule
        internal static string IsoStoreAskEveryTime = "conventionChoiceAskEveryTime"; // key for storing the users choice to be asked which convention to see at launch or not
        internal static string IsoStoreDefaultConventionId = "conventionChoiceDefaultId"; //key for storing the id of the convention to launch to without asking
        internal static string IsoStoreAppVersion = "appVersionNumber"; // store the app version
        internal static string IsoStoreConventionChanged = "changedConventionChoice"; // save whether the convention choice has updated
        internal static string IsoStoreNTimesOpened = "numberOfTimesOpened"; // save how many times it has been opened
        internal static string IsoStoreShowRatingPromptAgain = "showRatingPromptAgain"; // should we ask the user about rating again

        internal static string IsoStoreLocations = "locationValues";
        internal static string IsoStoreKinds = "kindValues";
        internal static string IsoStoreDays = "dayValues";

        #region category values
        
        public static List<string> GetLocations()
        {
            List<string> locationList = new List<string>();
            IsolatedStorageSettings.ApplicationSettings.TryGetValue(IsoStoreLocations + IsoStoreSettings.GetDefaultConvention(), out locationList);
            return locationList;
        }

        public static void SaveLocations(List<string> locations)
        {
            string key = IsoStoreLocations + IsoStoreSettings.GetDefaultConvention();
            ClearFromSettings(key);
            SaveToSettings<List<string>>(key, locations);
        }

        public static List<string> GetDays()
        {
            List<string> dayList = new List<string>();
            IsolatedStorageSettings.ApplicationSettings.TryGetValue(IsoStoreDays + IsoStoreSettings.GetDefaultConvention(), out dayList);
            return dayList;
        }

        public static void SaveDays(List<string> days)
        {
            string key = IsoStoreDays + IsoStoreSettings.GetDefaultConvention();
            ClearFromSettings(key);
            SaveToSettings<List<string>>(key, days);
        }

        public static List<string> GetKinds()
        {
            List<string> kindList = new List<string>();
            IsolatedStorageSettings.ApplicationSettings.TryGetValue(IsoStoreKinds + IsoStoreSettings.GetDefaultConvention(), out kindList);
            return kindList;
        }

        public static void SaveKinds(List<string> kinds)
        {
            string key = IsoStoreKinds + IsoStoreSettings.GetDefaultConvention();
            ClearFromSettings(key);
            SaveToSettings<List<string>>(key, kinds);
        }

        #endregion
            
        #region allow auto check for updates
        /// <summary>
        /// check if the user has auto schedule checking on. If the user has not set a value, return true
        /// </summary>
        /// <returns></returns>
        public static bool IsAllowedAutoCheckForUpdates()
        {
            //read from iso store
            bool bAllowAutoCheck = true;
            if (!IsolatedStorageSettings.ApplicationSettings.TryGetValue(IsoStoreAllowAutoUpdateCheck, out bAllowAutoCheck))
                return true;
            return bAllowAutoCheck;
        }

        /// <summary>
        /// turn auto checking on or off according to the user choice
        /// </summary>
        /// <param name="value"></param>
        /// <returns></returns>
        public static void SetAutoCheckForUpdates(bool value)
        {
            // save our value to the settings
            SaveToSettings<bool>(IsoStoreAllowAutoUpdateCheck, value);
        }
        #endregion

        #region is schedule available
        /// <summary>
        /// refers to the updateAvailable flag to see if a new schedule was found on our last check
        /// used by AboutPage
        /// </summary>
        /// <returns>bool true if a new schedule is available</returns>
        public static bool HasUpdateAvailable()
        {
            bool isAvailable = false;
            bool hasChecked = IsolatedStorageSettings.ApplicationSettings.TryGetValue(IsoStoreHasUpdateAvailable, out isAvailable);
            return isAvailable;
        }


        /// <summary>
        /// refers to the updateCheckFailed flag to see if a new schedule was found on our last check
        /// used by AboutPage
        /// </summary>
        /// <returns>bool true if we hit an error checking for a new schedule</returns>
        public static bool UpdateCheckFailed()
        {
            bool succeeded = false;
            bool hasChecked = IsolatedStorageSettings.ApplicationSettings.TryGetValue(IsoStoreUpdateCheckFailed, out succeeded);
            return succeeded;
        }


        #endregion

        #region update and creation times
        /// <summary>
        /// return a string showing the time we last updated the schedule
        /// </summary>
        /// <returns></returns>
        public static string GetLastUpdatedTime()
        {
            DateTime time = new DateTime(0);
            string conVersionKey = GetDefaultConvention() + IsoStoreLastUpdatedRecord;
            bool hasChecked = IsolatedStorageSettings.ApplicationSettings.TryGetValue(conVersionKey, out time);
            return (time != new DateTime(0)) ? time.ToShortDateString() : "unknown";
        }

        public static void SaveScheduleUpdatedDate(DateTime updateDate)
        {
            string conVersionKey = GetDefaultConvention() + IsoStoreLastUpdatedRecord;
            SaveToSettings<DateTime>(conVersionKey, updateDate);
        }


        /// <summary>
        /// return a string showing when the schedule we have now was created, for the current con
        /// </summary>
        /// <returns></returns>
        public static string GetScheduleCreationTime()
        {
            DateTime time = new DateTime(0);
            string conVersionKey = GetDefaultConvention() + IsoStoreScheduleCreationDate;
            bool hasChecked = IsolatedStorageSettings.ApplicationSettings.TryGetValue(conVersionKey, out time);
            return time.ToShortDateString();
        }

        public static void SaveScheduleCreationDate(DateTime creationDate)
        {
            string conVersionKey = GetDefaultConvention() + IsoStoreScheduleCreationDate;
            SaveToSettings<DateTime>(conVersionKey, creationDate);
        }


        #endregion

        #region current schedule version
        /// <summary>
        /// return an int showing the version of the schedule we have now 
        /// </summary>
        /// <returns></returns>
        public static int GetScheduleVersion()
        {
            int version = 0;
            string conVersionKey = GetDefaultConvention() + IsoStoreScheduleVersionNumber;
            bool hasChecked = IsolatedStorageSettings.ApplicationSettings.TryGetValue(conVersionKey, out version);
            return version;
        }

        /// <summary>
        /// save the version of the last updated schedule for a particular convention
        /// </summary>
        /// <param name="version"></param>
        public static void SaveScheduleVersion(int version)
        {
            string conVersionKey = GetDefaultConvention() + IsoStoreScheduleVersionNumber;
            SaveToSettings<int>(conVersionKey, version);
        }

        #endregion

        #region reminders 
        /// <summary>
        /// check if the user has reminders on. If no value found, return true 
        /// </summary>
        /// <returns></returns>
        public static bool IsAllowedSetReminders()
        {
            //read from iso store
            bool bAllowReminders = true;
            //if (!IsolatedStorageSettings.ApplicationSettings.TryGetValue(IsoStoreAllowSetReminders, out bAllowReminders))
            //    return true;
            return bAllowReminders;
        }

        /// <summary>
        /// save the users preference for setting reminders on events to iso store
        /// </summary>
        /// <param name="value"></param>
        public static void SetAllowReminders(bool value)
        {
            SaveToSettings<bool>(IsoStoreAllowSetReminders, value);
        }
        #endregion

        #region always ask at launch
        /// <summary>
        /// check if the user wants to see the launch choice every time they launch 
        /// </summary>
        /// <returns></returns>
        public static bool IsAskEveryTime()
        {
            //read from iso store
            bool bAskEveryTime = true;
            bool bUserHasSetChoice = IsolatedStorageSettings.ApplicationSettings.TryGetValue(IsoStoreAskEveryTime, out bAskEveryTime);
            return bAskEveryTime || ! bUserHasSetChoice;
        }

        /// <summary>
        /// save the users preference for launch choice to iso store
        /// </summary>
        /// <param name="value"></param>
        public static void SetAskEveryTime(bool value)
        {
            SaveToSettings<bool>(IsoStoreAskEveryTime, value);
        }
        #endregion

        #region convention being viewed

        // get the title of a convention as ConventionName[Convention.PAXEAST]
        public enum Convention {PAXEAST=0, PAXPRIME=1, PAXAUS=2};

        public static List<string> getConventionNames(bool testing)
        {
            if (testing)
                return new List<string>{ "PAX Test" };

            return new List<string> { "PAX East", "PAX Prime", "PAX Australia" }; 
        }

        public static List<string> ConventionNames
        {
            get { return getConventionNames(false); }
        }


        /// <summary>
        /// return an int showing the default convention to look at 
        /// </summary>
        /// <returns></returns>
        public static int GetDefaultConvention()
        {
            int version = 0;
            bool hasChecked = IsolatedStorageSettings.ApplicationSettings.TryGetValue(IsoStoreDefaultConventionId, out version);
            return hasChecked ? version : 0;
        }

        /// <summary>
        /// register the users choice for which convention to see on launching the app
        /// </summary>
        /// <param name="value"></param>
        public static void SaveDefaultConvention(int value)
        {
            SaveToSettings<int>(IsoStoreDefaultConventionId, value);
        }
        #endregion

        #region has convention choice changed
        /// <summary>
        /// check if the user wants to see the launch choice every time they launch 
        /// </summary>
        /// <returns></returns>
        public static bool IsConventionChanged()
        {
            //read from iso store
            bool bAskEveryTime = true;
            bool bUserHasSetChoice = IsolatedStorageSettings.ApplicationSettings.TryGetValue(IsoStoreConventionChanged, out bAskEveryTime);
            return bAskEveryTime || ! bUserHasSetChoice;
        }

        /// <summary>
        /// save the users preference for launch choice to iso store
        /// </summary>
        /// <param name="value"></param>
        public static void SetConventionChanged(bool value)
        {
            SaveToSettings<bool>(IsoStoreConventionChanged, value);
        }
        #endregion

        #region app version
        /// <summary>
        /// return an int showing the default convention to look at 
        /// </summary>
        /// <returns></returns>
        public static double GetAppVersion()
        {
            double version = 0;
            bool hasChecked = IsolatedStorageSettings.ApplicationSettings.TryGetValue(IsoStoreAppVersion, out version);
            return hasChecked ? version : 0;
        }

        /// <summary>
        /// register the users choice for which convention to see on launching the app
        /// </summary>
        /// <param name="value"></param>
        public static void SetAppVersion(double value)
        {
            SaveToSettings<double>(IsoStoreAppVersion, value);
        }
        #endregion

        #region app opening count
        /// <summary>
        /// return an int showing how many times the app has been opened
        /// </summary>
        /// <returns></returns>
        public static double GetOpenedCount()
        {
            double count = 0;
            bool hasChecked = IsolatedStorageSettings.ApplicationSettings.TryGetValue(IsoStoreNTimesOpened, out count);
            return hasChecked ? count : 0;
        }

        /// <summary>
        /// register the users choice for which convention to see on launching the app
        /// </summary>
        /// <param name="value"></param>
        public static void SetOpenedCount(double value)
        {
            SaveToSettings<double>(IsoStoreNTimesOpened, value);
        }

        public static bool GetShowRatingState()
        {
            bool doShow = true;
            bool hasChecked = IsolatedStorageSettings.ApplicationSettings.TryGetValue(IsoStoreShowRatingPromptAgain, out doShow);
            return hasChecked ? doShow : true;
        }

        public static void SetShowRatingState(bool value)
        {
            SaveToSettings<bool>(IsoStoreShowRatingPromptAgain, value);
        }

        #endregion

        #region slqite db
        public static string DBName = "Events.sqlite";
        public static string DBAssembly = "PAX7";
      
        #endregion 

        #region generic functions
        /// <summary>
        /// save a value to settings in isostore
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        public static void SaveToSettings<Type>(string key, Type value)
        {
            if (IsolatedStorageSettings.ApplicationSettings.Contains(key) == true)
            {
                // key already exists, remove it  
                IsolatedStorageSettings.ApplicationSettings.Remove(key);
            }
            IsolatedStorageSettings.ApplicationSettings.Add(key, value);
            IsolatedStorageSettings.ApplicationSettings.Save();
        }

        public static void ClearFromSettings(string key)
        {
            IsolatedStorageSettings.ApplicationSettings.Remove(key);
        }

        #endregion
    }
}
