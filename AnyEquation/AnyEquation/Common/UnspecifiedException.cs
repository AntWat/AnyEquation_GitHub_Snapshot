using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace AnyEquation.Common
{
    public class UnspecifiedException : Exception
    {
        public UnspecifiedException(string message="Unspecified Exception") : base(message)
        {
            // TODO: Review all uses of this
        }
    }
}
