using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AnyEquation.Common
{
    public class AwaitableContentPage : ContentPage
    {
        // Use this to wait on the page to be finished with/closed/dismissed
        public Task PageClosedTask { get { return tcs.Task; } }

        private bool _endTaskOnDisappearing=false;
        public bool EndTaskOnDisappearing
        {
            get { return _endTaskOnDisappearing; }
            set { _endTaskOnDisappearing = value; }
        }


        private TaskCompletionSource<bool> tcs { get; set; }

        public AwaitableContentPage()
        {
            tcs = new System.Threading.Tasks.TaskCompletionSource<bool>();
        }

        // Either override OnDisappearing 
        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            if (EndTaskOnDisappearing)
            {
                tcs.SetResult(true);
            }
        }

        // Or provide your own PopAsync function so that when you decide to leave the page explicitly the TaskCompletion is triggered
        public async Task PopAwaitableAsync()
        {
            await Navigation.PopAsync();
            tcs.SetResult(true);
        }

        public async Task PopAwaitableModalAsync()
        {
            await Navigation.PopModalAsync();
            tcs.SetResult(true);
        }
    }
}
