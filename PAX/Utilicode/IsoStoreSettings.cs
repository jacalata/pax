using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.IO.IsolatedStorage;

namespace PAX7.Utilicode
{
    static class IsoStoreSettings
    {

        internal static string IsoStoreLastUpdatedRecord = "lastUpdated"; // the name for the value we look at to check the last schedule update date
        internal static string IsoStoreScheduleCreationDate = "scheduleCreationDate"; // the name for the value we look at to check the creation date of the current info
        internal static string IsoStoreHasUpdateAvailable = "hasUpdateAvailable";// the name for the value we look at to check if a new update is available
        internal static string IsoStoreAllowAutoUpdateCheck = "allowAutoUpdating"; // the name for the value to look at to see if we should check for updates 
        internal static string IsoStoreAllowSetReminders = "allowSetReminders"; // the key for the value of whether the user wants reminders


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
        /// return a string showing the time we last updated the schedule
        /// </summary>
        /// <returns></returns>
        public static string GetLastUpdatedTime()
        {
            DateTime time = new DateTime(0);
            bool hasChecked = IsolatedStorageSettings.ApplicationSettings.TryGetValue(IsoStoreLastUpdatedRecord, out time);
            return (time != new DateTime(0)) ? time.ToShortDateString() : "unknown";
        }

        /// <summary>
        /// return a string showing when the schedule we have now was created
        /// </summary>
        /// <returns></returns>
        public static string GetScheduleCreationTime()
        {
            DateTime time = new DateTime(0);
            bool hasChecked = IsolatedStorageSettings.ApplicationSettings.TryGetValue(IsoStoreScheduleCreationDate, out time);
            return (time != new DateTime(0)) ? time.ToShortDateString() : "unknown";
        }



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

    }
}
