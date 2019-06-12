using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnyEquation.Equations.Common;

namespace AnyEquation.Equations.Model
{
    public class DisplayQuantity : Quantity
    {
        public DisplayQuantity() { }

        public DisplayQuantity(double value, CalcStatus calcStatus, string message, KnownUOM kownUOM) :
            base(value, calcStatus, message)
        {
            KownUOM = kownUOM;
        }

        public KnownUOM KownUOM { get; set; }
        public override UOM GetUOM()
        {
            return KownUOM;
        }

        public override void CopyMe(Quantity tgtQuantity)
        {
            base.CopyMe(tgtQuantity);

            if (tgtQuantity is DisplayQuantity tgtDisplayQuantity)
            {
                tgtDisplayQuantity.KownUOM = KownUOM;
            }
        }

    }
}
