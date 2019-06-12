using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AnyEquation.Common
{
    public class NotifyableContentView : ContentView
    {
        /// <summary>
        /// Message sent to this class when it is about to be appear.  
        /// This is the closes we can get to an equivalent of the ContentPage OnAppearing life-cycle event
        /// </summary>
        public virtual void AboutToAppear()
        {

        }

        public virtual void NotVisible()
        {

        }
    }
}
