using System;
namespace PAX7.Utilicode
{
    interface IDatabase
    {
        void Delete(string statement);
        int Insert<T>(T obj, string statement) where T : new();
        global::System.Collections.ObjectModel.ObservableCollection<T> SelectObservableCollection<T>(string statement) where T : new();
    }
}
