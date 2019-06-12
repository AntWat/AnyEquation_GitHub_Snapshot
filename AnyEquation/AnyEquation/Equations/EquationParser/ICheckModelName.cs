using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnyEquation.Equations.EquationParser
{
    public interface ICheckModelName
    {
        bool CheckModelName(String modelName, String libPackageName);
    }
}
