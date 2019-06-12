using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnyEquation.Equations.Database
{
    public abstract class ImportedRow
    {
        public abstract bool OkToUse();

        /// This will be called after the table row has been imported, to allow any post-processing needed
        public abstract void DoneImport();

    }
}
