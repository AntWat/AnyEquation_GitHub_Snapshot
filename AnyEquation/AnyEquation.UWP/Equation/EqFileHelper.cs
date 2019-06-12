using AnyEquation.Equations.Database;
using AnyEquation.UWP.Equation;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Windows.Storage;
using Xamarin.Forms;

[assembly: Dependency(typeof(EqFileHelper))]
namespace AnyEquation.UWP.Equation
{
    class EqFileHelper : IEqFileHelper
    {
        async Task<string> IEqFileHelper.GetDBPathAndCreateIfNotExists(string databaseName)
        {
            try
            {
                var installFolder = Windows.ApplicationModel.Package.Current.InstalledLocation;
                var installDbPath = Path.Combine(installFolder.Path + "\\Databases", databaseName); // FILE NAME TO USE WHEN COPIED

                FileInfo fInfo = new FileInfo(installDbPath);
                if (!fInfo.Exists)
                {
                    return null;
                }

                var userFolder = Windows.Storage.ApplicationData.Current.LocalFolder;
                var userDbPath = Path.Combine(userFolder.Path, databaseName); // FILE NAME TO USE WHEN COPIED

                bool replaceUserFile = false;
                fInfo = new FileInfo(userDbPath);
                if (!fInfo.Exists)
                {
                    replaceUserFile = true;
                }
                else
                {
                    // TODO: Decide to replace user file (if they never edit it), or merge user file with install file?
                    replaceUserFile = true;
                    StorageFile userDbFile = await StorageFile.GetFileFromPathAsync(userDbPath);
                    await userDbFile.DeleteAsync(StorageDeleteOption.Default);
                }

                if (replaceUserFile)
                {
                    StorageFile installDbFile = await StorageFile.GetFileFromPathAsync(installDbPath);
                    await installDbFile.CopyAsync(userFolder);
                }

                fInfo = new FileInfo(userDbPath);
                if (!fInfo.Exists)
                {
                    return null;
                }

                return userDbPath;
            }
            catch (System.Exception ex)
            {
                throw;
            }
        }
    }
}