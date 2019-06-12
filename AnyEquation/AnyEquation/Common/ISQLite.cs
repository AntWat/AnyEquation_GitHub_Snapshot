using System;
using SQLite;

namespace AnyEquation.Common
{
    public enum enAppTypes
    {
        App_DoList
    }

    public interface ISQLite
    {
        SQLiteConnection GetConnection(enAppTypes appType);
    }
}

