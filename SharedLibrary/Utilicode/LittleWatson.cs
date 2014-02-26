using System;
using System.Net;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Ink;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Collections.Generic;
using System.Windows.Shapes;
using System.IO.IsolatedStorage;
using System.IO;
using Microsoft.Phone.Tasks;

namespace PAX7
{
    public class LittleWatson
    {

        const string filename = "LittleWatson.txt";

        public static void LogInfo(string extra)
        {
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    using (TextWriter output = new StreamWriter(store.OpenFile(filename, FileMode.Append)))
                    {
                        output.WriteLine("LOGINFO: " + extra + ": timestamp="+DateTime.Now);
                    }
                }
            }
            catch (Exception)
            {
            }
        }

        public static void ReportException(Exception ex, string extra="no extra info included")
        {
            try
            {
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    List<string> history = new List<string>();
                    // should pull out all lines saying 'LOGINFO' or 'EXCEPTIONINFO' 
                    // so I at least know how much was wiped before this exception was recorded
                    using (TextReader oldFileReader = new StreamReader(store.OpenFile(filename, FileMode.Open)))
                    {
                        string line = oldFileReader.ReadLine();
                        while (line != null)
                        {
                            if (line.Contains("LOGINFO") || line.Contains("EXCEPTIONINFO"))
                                history.Add(line);
                            line = oldFileReader.ReadLine();
                        }
                    }
                    SafeDeleteFile(store);

                    using (TextWriter output = new StreamWriter(store.CreateFile(filename)))
                    {
                        foreach (string line in history)
                        {
                            output.WriteLine(line);
                        }
                        output.WriteLine("EXCEPTIONINFO: " + extra + ": timestamp="+DateTime.Now);
                        output.WriteLine(ex.Message);
                        output.WriteLine(ex.StackTrace);
                    }
                }
            }
            catch (Exception)
            {
            }
        }
 
        public static void CheckForPreviousException()
        {
            try
            {
                string contents = null;
                using (var store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    if (store.FileExists(filename))
                    {
                        using (TextReader reader = 
                            new StreamReader(store.OpenFile(filename, FileMode.Open, FileAccess.Read, FileShare.None)))
                        {
                                contents = reader.ReadToEnd();                                
                        }
                        SafeDeleteFile(store);
                    }
                }
                if (contents != null)
                {

                    if (MessageBox.Show("A problem occurred the last time you ran this application. Would you like to send an email to report it? Please?", "Problem Report", MessageBoxButton.OKCancel) == MessageBoxResult.OK)
                    {

                        EmailComposeTask email = new EmailComposeTask();
                        email.To = "jacalata@live.com";
                        email.Subject = "PAX Digital Assistant auto-generated problem report";
                        email.Body = contents;
                        SafeDeleteFile(IsolatedStorageFile.GetUserStoreForApplication()); // line added 1/15/2011
                        email.Show();
                    }
                    else
                    { //don't ask again
                        SafeDeleteFile(IsolatedStorageFile.GetUserStoreForApplication()); // line added 1/15/2011
                    }
                }
            }
            catch (Exception)
            {
            }
            finally
            {
                SafeDeleteFile(IsolatedStorageFile.GetUserStoreForApplication());
            }
        }
 
        private static void SafeDeleteFile(IsolatedStorageFile store)
        {
            try
            {
                store.DeleteFile(filename);
            }
            catch (Exception ex)
            {
                ReportException(ex, "it's coming from inside the house!");
            }
        }
    }
}
