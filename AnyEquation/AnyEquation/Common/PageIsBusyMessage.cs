using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AnyEquation.Common
{
    class PageIsBusyMessage : AntMessageBase
    {
        public PageIsBusyMessage(bool pageIsBusy)
        {
            PageIsBusy = pageIsBusy;
        }

        public bool PageIsBusy { get; set; }
    }
}
