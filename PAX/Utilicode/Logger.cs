using System;
using System.Text;
using System.Diagnostics;
using System.IO.IsolatedStorage;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Xml.Linq;
using System.Xml;

/*
 * http://wpclogger.codeplex.com/
 */

namespace PAX7.Utilicode
{
    /// <summary>
    /// Custom LogLevel enum accessible from the rest of the app
    /// </summary>
    public enum LogLevel
    {
        debug,
        info,
        warn,
        error,
        critical
    }

    public enum LogActivity
    {
        schedulecheck,
        schedulefound,
        scheduleerror,
        scheduleupdated,
        scheduleload,
        schedulesearch,
        staradd,
        starremove,
        sendschedule,
        settingsview,
        mapview,
        applaunch
    };




    /// <summary>
    /// WPClogger v0.3
    /// Author: Jay Bennett
    /// </summary>
    public class WPClogger
    {
        public static string[] LogMessages =
        {
            "schedulecheck",
            "schedulefound",
            "scheduleerror",
            "scheduleupdated",
            "scheduleload",
            "schedulesearch",
            "staradd",
            "starremove",
            "sendschedule",
            "settingsview",
            "mapview",
            "applaunch"
        };

    }
}