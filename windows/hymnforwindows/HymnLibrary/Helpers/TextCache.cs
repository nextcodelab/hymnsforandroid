using SQLite;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Threading.Tasks;

namespace HymnLibrary.Helpers
{
    public class TextCache
    {
        const string TEXTCACHE = "txtcaches";
        
        static string _path = Path.Combine(DatabaseDirectory.Dir, TEXTCACHE);
        static string Init()
        {
            _path = Path.Combine(DatabaseDirectory.Dir, TEXTCACHE);
            if (!Directory.Exists(_path))
                Directory.CreateDirectory(_path);
            return _path;
        }
        #region Text
        public static void SaveStringToStorageFolder(string fileName, string content)
        {

            var filePath = Path.Combine(Init(), fileName);
            File.WriteAllText(filePath, content);
        }
        public static async Task<string> ReadSavedStringAsync(string fileName)
        {
            string readText;
            var filePath = Path.Combine(Init(), fileName);
            using (StreamReader readtext = new StreamReader(filePath))
            {
                readText = await readtext.ReadToEndAsync();
            }
            return readText;
        }
        public static bool IsFileExist(string fileName)
        {
            var filePath = Path.Combine(Init(), fileName);
            return File.Exists(filePath);
        }
        #endregion
        #region DataCache
        public static void SaveCache(DataCache item)
        {
            using (var session = new SQLite.SQLiteConnection(Path.Combine(Init(), nameof(DataCache) + ".db")))
            {
                session.CreateTable<DataCache>();
                session.GetMapping<DataCache>();

                var existItem = session.Table<DataCache>().Where(p => p.UniqueId == item.UniqueId).FirstOrDefault();
                if (existItem != null)
                {
                    item.Id = existItem.Id;
                    session.Update(item);
                }
                else
                {
                    session.Insert(item);
                }
            }
        }
        public static DataCache GetCache(string uniqueId)
        {
            using (var session = new SQLite.SQLiteConnection(Path.Combine(Init(), nameof(DataCache) + ".db")))
            {
                session.CreateTable<DataCache>();
                session.GetMapping<DataCache>();

                var existItem = session.Table<DataCache>().Where(p => p.UniqueId == uniqueId).FirstOrDefault();
                return existItem;
            }
        } 
        #endregion
    }
    public class DataCache
    {
        [PrimaryKey, AutoIncrement]
        public int Id { get; set; }
        public string UniqueId { get; set; }
        public byte[] Data { get; set; }
    }
}
