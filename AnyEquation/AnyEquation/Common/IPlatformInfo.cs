using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnyEquation.Common
{
    public interface IPlatformInfo
    {
        string GetModel();

        string GetVersion();

        string GetAppVersion();
    }
}
