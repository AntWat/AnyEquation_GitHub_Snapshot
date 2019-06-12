using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnyEquation.Equations.Model
{
    public interface IContent
    {
        string Name{ get; }
        string PackageName { get; }
    }
}
