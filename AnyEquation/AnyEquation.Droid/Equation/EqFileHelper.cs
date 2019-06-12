using AnyEquation.Equations.Database;
using System.IO;
using AnyEquation.Droid.Equation;
using System.Threading.Tasks;
using Xamarin.Forms;

[assembly: Dependency(typeof(EqFileHelper))]
namespace AnyEquation.Droid.Equation
{
    class EqFileHelper : IEqFileHelper
    {
        async Task<string> IEqFileHelper.GetDBPathAndCreateIfNotExists(string databaseName)
        {
            try
            {
                var docFolder = System.Environment.GetFolderPath(System.Environment.SpecialFolder.Personal);
                var dbFile = Path.Combine(docFolder, databaseName); // FILE NAME TO USE WHEN COPIED
                if (!File.Exists(dbFile))
                {
                    FileStream writeStream = new FileStream(dbFile, FileMode.OpenOrCreate, FileAccess.Write);
                    await Forms.Context.Assets.Open(databaseName).CopyToAsync(writeStream);
                }
                return dbFile;
            }
            catch (System.Exception ex)
            {
                throw;
            }
        }
    }
}