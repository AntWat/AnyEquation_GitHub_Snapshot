using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnyEquation.Equations.Database
{
    public interface IEqFileHelper
    {
        Task<String> GetDBPathAndCreateIfNotExists(string databaseName);
    }
}
