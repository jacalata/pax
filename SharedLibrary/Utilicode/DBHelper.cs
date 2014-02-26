using SQLiteClient;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel; //observablecollection
using System.IO; //streams
using System.IO.IsolatedStorage;
using System.Linq;
using System.Net;
using System.Text;
using System.Windows;
using System.Xml.Linq;
using PAX7.Model;

namespace PAX7.Utilicode
{  
    // taken from http://dotnetslackers.com/articles/silverlight/Windows-Phone-7-Native-Database-Programming-via-Sqlite-Client-for-Windows-Phone.aspx
    public class DBHelper : IDatabase
    {
        #region private variables
        private String _dbName = "Events.sqlite";        
        private static SQLiteConnection db = null;
        System.Threading.Mutex mutex;
        #endregion

        #region private setup/teardown methods
       
        private void Open()
        {
            try
            {
                if (db == null)
                {
                    db = new SQLiteConnection(_dbName);
                    db.Open();
                }
            }
            catch (Exception e)
            {
                LittleWatson.ReportException(e);
            }
        }

        private void Close()
        {
            try
            {
                if (db != null)
                {
                    db.Dispose();
                    db = null;
                }
            }
            catch (Exception e)
            {
                LittleWatson.ReportException(e);
            }
        }

        
        #endregion

        /// <summary>
        /// Constructor - open db and create table if none exists
        /// </summary>
        public DBHelper()
        {
            //copy db from xap to isolated storage 
            IsolatedStorageFile store =IsolatedStorageFile.GetUserStoreForApplication();
            if (!store.FileExists(_dbName))
            {
                CopyFromContentToStorage();
            }
            Open();
        }

        ~DBHelper()
        {
            Close();
        }

        #region copying resource db to isolated storage
        
        private void CopyFromContentToStorage()
        {
            var mutex = new System.Threading.Mutex(false, _dbName);
            mutex.WaitOne();


            try
            {
                using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
                {
                    // create journal file so we don't get a permission error later
                    string uri = _dbName + "-journal";
                    IsolatedStorageFileStream journalStream = store.CreateFile(uri);
                    journalStream.Close();

                    // copy pre-created table from resources to storage
                    Uri dbUri = new Uri("Resources/"+_dbName, UriKind.Relative);
                    System.IO.Stream src = System.Windows.Application.GetResourceStream(dbUri).Stream;                
                    IsolatedStorageFileStream dest = new IsolatedStorageFileStream(_dbName,
                            System.IO.FileMode.OpenOrCreate,
                            System.IO.FileAccess.Write, store);
                    src.Position = 0;
                    CopyStream(src, dest);
                    dest.Flush();
                    dest.Close();
                    src.Close();
                    dest.Dispose();
                }
            }
            catch (Exception e)
            {
                LittleWatson.ReportException(e, "Error copying dbfile from resources to storage");
            }
            finally
            {
                mutex.ReleaseMutex();
            }
            using (IsolatedStorageFile store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                // create journal file so we don't get a permission error later
                string uri = _dbName + "-journal";
                IsolatedStorageFileStream file = store.OpenFile(uri, FileMode.OpenOrCreate);
                file.Close();
            }

        }

        private static void CopyStream(System.IO.Stream input,
                                        IsolatedStorageFileStream output)
        {
            byte[] buffer = new byte[32768];
            long TempPos = input.Position;
            int readCount;
            do
            {
                readCount = input.Read(buffer, 0, buffer.Length);
                if (readCount > 0)
                {
                    output.Write(buffer, 0, readCount);
                }
            } while (readCount > 0);
            input.Position = TempPos;
        }
        #endregion
   

        #region sql operations
        
        /// <summary>
        /// Insert a new object, mapped to the fields in the statement
        /// </summary>
        /// <typeparam name="T">object type</typeparam>
        /// <param name="obj">the object to insert</param>
        /// <param name="statement">the insert statement with field mappings</param>
        /// <returns>the number of items created?? 0 on failure</returns>
        public int Insert<T>(T obj, string statement) where T : new()
        {
            var mutex = new System.Threading.Mutex(false, _dbName);
            mutex.WaitOne();
            try
            {
                Open();
                SQLiteCommand cmd = db.CreateCommand(statement);
                int rec = cmd.ExecuteNonQuery(obj);
                return rec;
            }
            catch (SQLiteException ex)
            {
                LittleWatson.ReportException(ex, "Sqlite Insert failed with error " + ex.ErrorCode + ": " + ex.Message);
                return 0;
            }
            catch (NullReferenceException ex)
            {
                LittleWatson.ReportException(ex, "Sqlite Insert failed: " + ex.Message);
                return 0;
            }
            finally
            {
                Close();
                mutex.ReleaseMutex();
            }
        }
        
        
        /// <summary>
        /// Delete operation - opens db, executes statement, closes db
        /// </summary>
        /// <param name="statement">delete statement</param>
        public void Delete(string statement)
        {
            var mutex = new System.Threading.Mutex(false, _dbName);
            mutex.WaitOne();
            try
            {
                Open();
                SQLiteCommand cmd = db.CreateCommand(statement);
                cmd.ExecuteNonQuery();
            }
            catch (SQLiteException ex)
            {
                LittleWatson.ReportException(ex, "Sqlite Deletion failed: " + ex.Message);
            }
            catch (NullReferenceException ex)
            {
                LittleWatson.ReportException(ex, "Sqlite Insert failed: " + ex.Message);
            }
            finally
            {
                Close();
                mutex.ReleaseMutex();
            }
        }
      

        /// <summary>
        /// Query operation - opens db, executes query, closes db
        /// </summary>
        /// <typeparam name="T">the type of object to create with the returned fields</typeparam>
        /// <param name="statement">the select statement</param>
        /// <returns>a (potentially null) List of objects T</returns>
        private List<T> SelectList<T>(String statement) where T : new()
        {
            var mutex = new System.Threading.Mutex(false, _dbName);
            mutex.WaitOne();
            try
            {
                Open();
                SQLiteCommand cmd = db.CreateCommand(statement);
                var lst = cmd.ExecuteQuery<T>();
                return lst.ToList<T>();
            }
            catch (SQLiteException ex)
            {
                LittleWatson.ReportException(ex, "Sqlite select list failed: " + ex.Message);
                return null;
            }
            catch (NullReferenceException ex)
            {
                LittleWatson.ReportException(ex, "Sqlite Insert failed: " + ex.Message);
                return null;
            }
            finally
            {
                Close();
                mutex.ReleaseMutex();
            }
        }
      

        /// <summary>
        /// helper method - calls SelectList and translates the result into an ObservableCollection
        /// </summary>
        /// <typeparam name="T">type of object in the collection</typeparam>
        /// <param name="statement">query statement</param>
        /// <returns>an ObservableCollection, possibly null or empty</returns>
        public ObservableCollection<T> SelectObservableCollection<T>(String statement)
            where T : new()
        {
            using (var store = IsolatedStorageFile.GetUserStoreForApplication())
            {
                bool exists = store.FileExists(_dbName);
                if (!exists)
                {
                    LittleWatson.LogInfo("db file does not exist in Isolated Storage");
                    return null;
                }
            }
            List<T> lst = SelectList<T>(statement);
            ObservableCollection<T> oc = new ObservableCollection<T>();
            if (lst == null)
                return oc;
            foreach (T item in lst)
            {
                oc.Add(item);
            }
            return oc;
        }

        #endregion

     }
}
