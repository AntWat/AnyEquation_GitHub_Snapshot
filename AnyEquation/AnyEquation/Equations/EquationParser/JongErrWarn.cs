using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnyEquation.Equations.EquationParser
{
    public enum ErrWStat
    {
        NONE,
        INFO,
        INPUT_OR_CALC_PROBLEM,
        UNKNOWN_ERROR,
        CRITICAL_ERROR
    }

    public class JongErrWarn
    {
        public void Init(ErrWStat errWStat, String strErrWarn, bool alreadyLogged)
        {
            ErrWStat = errWStat;
            ErrWarnString = strErrWarn;
            AlreadyLogged = alreadyLogged;
        }

        public ErrWStat ErrWStat { get; set; }
        public string ErrWarnString { get; set; }
        public bool AlreadyLogged { get; set; }

    }
}
