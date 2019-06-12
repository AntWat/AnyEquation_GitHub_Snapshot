using SQLite;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnyEquation.Equations.Common
{
    abstract class SqLiteHelper
    {

        public class CName
        {
            public string name { get; set; }
        }

        public static IList<string> GetTableNames(SQLiteConnection database)
        {
            List<CName> tables = database.Query<CName>("SELECT name FROM sqlite_master WHERE type='table'");
            if (tables == null) return null;

            return (from t in tables select t.name).ToList();
        }
    }
}
