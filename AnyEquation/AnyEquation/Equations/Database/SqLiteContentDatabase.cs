using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Threading;
using Xamarin.Forms;
using AnyEquation.Equations.Common;
using AnyEquation.Equations.Model;

namespace AnyEquation.Equations.Database
{
    public class SqLiteContentDatabase
    {
        #region ------------ Constants ------------

        public const string STR_DbFormatNumber = "DbFormatNumber";
        public const string STR_PackageName = "PackageName";
        public const string STR_LibraryName = "LibraryName";
        public const string STR_LibraryDescription = "LibraryDescription";

        #endregion ------------ Constants ------------


        #region ------------ Fields and Properties ------------

        public SQLiteConnection SQLiteConnection { get; set; }

        public string Name { get; set; }
        public string Description { get; set; }
        public string PackageName { get; set; }

        private IDictionary<string, string> _infos = new Dictionary<string, string>();

        public string GetInfoVal(string key, string defaultVal="")
        {
            if (_infos.ContainsKey(key))
            {
                return _infos[key];
            }
            return defaultVal;
        }

        // -------------------
        static object locker = new object();

        // -------------------
        public string DbFormatNumber { get; set; }

        #endregion ------------ Fields and Properties ------------


        #region ------------ Database stuff ------------

        public static async Task<SqLiteContentDatabase> LoadDatabase(string sqLiteDbFilename)
        {
            SqLiteContentDatabase sqLiteContentDatabase = new SqLiteContentDatabase();
            await sqLiteContentDatabase.LoadTheDatabase(sqLiteDbFilename);
            return sqLiteContentDatabase;
        }

        public async Task<bool> LoadTheDatabase(string sqLiteDbFilename)
        {
            try
            {
                string dbPath = await DependencyService.Get<IEqFileHelper>().GetDBPathAndCreateIfNotExists(sqLiteDbFilename);

                SQLiteConnection = new SQLite.SQLiteConnection(dbPath);

                IList<string> tbls = SqLiteHelper.GetTableNames(SQLiteConnection);  // Useful for debugging

                _infos = ReadInfoTable(SQLiteConnection);
                
                DbFormatNumber = _infos[STR_DbFormatNumber];
                Name = _infos[STR_LibraryName];
                Description = _infos[STR_LibraryDescription];
                PackageName = _infos[STR_PackageName];

                //await Task.Delay(5000);       // Simulate a delay for Debugging

                return true;
            }
            catch (Exception ex)
            {
                throw;
            }
        }

        // ----------------- ReadInfoTable

        public class TblInfo_Row
        {
            public string Key { get; set; }
            public string Value { get; set; }
        }

        private static IDictionary<string, string> ReadInfoTable(SQLiteConnection database)
        {
            try
            {
                IDictionary<string, string> infos = new Dictionary<string, string>();

                lock (locker)
                {
                    var vInfos = database.Query<TblInfo_Row>("SELECT * FROM [tblInfo]").ToList();

                    foreach (var item in vInfos)
                    {
                        infos.Add(item.Key, item.Value);
                    }
                }
                return infos;

            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                // TODO
                throw;
            }

        }

        #endregion ------------ Database stuff ------------

    }

    public class SqLiteContentTable<TableRow> where TableRow : ImportedRow, new()
    {
        //public IList<TableRow> ContentRows { get; set; }

        // -------------------
        static object locker = new object();

        // ----------------- ReadContentTable

        public static IList<TableRow> ReadContentTable(SqLiteContentDatabase sqLiteContentDatabase, string contentTableName)
        {
            lock (locker)
            {
                IList<TableRow> rows = sqLiteContentDatabase.SQLiteConnection.Query<TableRow>(string.Format("SELECT * FROM [{0}]", contentTableName));

                foreach (ImportedRow row in rows)
                {
                    row.DoneImport();
                }

                return rows;
            }
        }


    }
}