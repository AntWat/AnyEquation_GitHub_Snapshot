using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnyEquation.Equations.Common;

namespace AnyEquation.Equations.Model
{
    public class Variable : SingleValue
    {
        public Variable(string name) :
            base(new CalcQuantity(), name, "", null)
        {
        }

        public override string Text { get { return Name; } }
    }
}
