using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnyEquation.Equations.Model.Info
{
    /// <summary>
    /// Used to hold general information about equation variables when they are not yet part of a calculation
    /// </summary>
    public class VarInfo
    {
        public VarInfo(string name, string description, ParamType paramType)
        {
            Name = name;
            Description = description;
            ParamType = paramType;
        }
        public string Name { get; set; }
        public string Description { get; set; }
        public ParamType ParamType { get; set; }
    }
}
