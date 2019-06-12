using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Xamarin.Forms;

namespace AnyEquation.Common
{
    class Utils
    {
        public static string NewLine = Environment.NewLine;

        // -----------------
        private static int appIsBusyCount = 0;     // Ref counter to track appIsBusy. Any non-zero value means busy
        private static IList<int> _appBusyQueue = new List<int>();
        private static Object _appBusyLock = new Object();
        private static void ProcessAppBusyQueue()
        {
            lock (_appBusyLock)
            {
                // Note: We can't just use foreach here in case other threads are still adding to the queue.

                // Move the currently known items to a new queue in case any more are added whilst processing
                int ct = _appBusyQueue.Count;      // The queue at this point, which this routine will process.
                IList<int> appBusyQueue = new List<int>();
                for (int i = 0; i < ct; i++)
                {
                    appBusyQueue.Add(_appBusyQueue[0]);
                    _appBusyQueue.RemoveAt(0);
                }

                foreach (int item in appBusyQueue)
                {
                    appIsBusyCount += item;
                }
                bool isBusy = (appIsBusyCount > 0);

                Device.BeginInvokeOnMainThread(() => {
                    if ((bool)App.Current.Resources["ShowAppBusy"] != isBusy)
                    {
                        App.Current.Resources["ShowAppBusy"] = isBusy;
                    }
                });
            }
        }

        public static void IncrementAppIsBusy()
        {
            ChangeAppIsBusy(1);
        }
        public static void DecrementAppIsBusy()
        {
            ChangeAppIsBusy(-1);
        }
        private static void ChangeAppIsBusy(int increment)
        {
            _appBusyQueue.Add(increment);

            // Notify the gui that the app is busy, but wait a short while first in case the operation is very short
            Device.StartTimer(TimeSpan.FromSeconds(0.1), () =>
            {
                ProcessAppBusyQueue();
                return false;
            });
        }


        // -----------------
        //public static void StartPageIsBusy(Page page, Action<PageIsBusyMessage> action)
        //{
        //    SubscribeToPageIsBusy(page, action);
        //    IncrementPageIsBusy(page, ResetCounter:true);
        //}
        //public static void StopPageIsBusy(Page page)
        //{
        //    DecrementPageIsBusy(page);
        //    ProcessPageBusyQueue(page);
        //    UnSubscribeToPageIsBusy(page);
        //}

        //public static void SubscribeToPageIsBusy(Page page, Action<PageIsBusyMessage> action)
        //{
        //    AntMessenger<PageIsBusyMessage>.Default().Register(page, action, /*invokeOnUiThread*/true, page);
        //}

        //public static void UnSubscribeToPageIsBusy(Page page)
        //{
        //    AntMessenger<PageIsBusyMessage>.Default().UnRegister(page, page);
        //}

        //// -----------------
        //private static IDictionary<Page, int> _pageIsBusyCount = new Dictionary<Page, int>();     // Ref counter to track appIsBusy. Any non-zero value means busy

        //private static IDictionary<Page, IList<int>> _pageBusyQueues = new Dictionary<Page, IList<int>>();
        //private static Object _pageBusyLock1 = new Object();
        //private static void ProcessPageBusyQueue(Page page)
        //{
        //    lock (_pageBusyLock1)
        //    {
        //        // Note: We can't just use foreach here in case other threads are still adding to the queue.

        //        // Move the currently known items to a new queue in case any more are added whilst processing
        //        int ct = _pageBusyQueues[page].Count;      // The queue at this point, which this routine will process.
        //        IList<int> busyQueue = new List<int>();
        //        for (int i = 0; i < ct; i++)
        //        {
        //            busyQueue.Add(_pageBusyQueues[page][0]);
        //            _pageBusyQueues[page].RemoveAt(0);
        //        }

        //        foreach (int item in busyQueue)
        //        {
        //            _pageIsBusyCount[page] += item;
        //        }
        //        bool isBusy = (_pageIsBusyCount[page] > 0);

        //        AntMessenger<PageIsBusyMessage>.Default().Send(new PageIsBusyMessage(isBusy), page);
        //    }
        //}

        //// Note: This facility was constructed as a queue to handle multiple calls to IncrementPageIsBusy and DecrementPageIsBusy, but the need to run
        //// actions on the UI thread poses a danger that the events could be processed out of order.  They will be made in the correct order nut
        //// I don't know if that guarantees the order the UI will process them.
        //// For this reason, IncrementPageIsBusy and DecrementPageIsBusy are private and not recommended for multiple use.
        //private static void IncrementPageIsBusy(Page page, bool ResetCounter = false)
        //{
        //    ChangePageIsBusy(1, page, ResetCounter);
        //}
        //private static void DecrementPageIsBusy(Page page)
        //{
        //    ChangePageIsBusy(-1, page);
        //}

        //private static Object _pageBusyLock2 = new Object();
        //private static void ChangePageIsBusy(int increment, Page page, bool ResetCounter = false)
        //{
        //    lock (_pageBusyLock2)
        //    {
        //        if (!_pageBusyQueues.ContainsKey(page)) _pageBusyQueues.Add(page, new List<int>());
        //        if (!_pageIsBusyCount.ContainsKey(page)) _pageIsBusyCount.Add(page, 0);

        //        if (ResetCounter)
        //        {
        //            _pageIsBusyCount[page] = 0;
        //        }
        //        _pageBusyQueues[page].Add(increment);

        //        // Notify the gui that the app is busy, but wait a short while first in case the operation is very short
        //        Device.StartTimer(TimeSpan.FromSeconds(0.1), () =>
        //        {
        //            ProcessPageBusyQueue(page);
        //            return false;
        //        });
        //    }
        //}

        // -----------------
        public static double parseToDouble(object oVal, double defaultVal)
        {
            try
            {
                double dVal = double.NaN;

                if (oVal == null)
                {
                    return double.NaN;
                }
                else if (oVal is string)
                {
                    if (!Double.TryParse(oVal as string, out dVal))
                        dVal = defaultVal;
                }
                else if (oVal is int)
                {
                    dVal = (double)(int)oVal;
                }
                else if (oVal is double)
                {
                    dVal = (double)oVal;
                }

                return dVal;
            }
            catch (Exception ex)
            {
                throw;
            }
        }


        // -----------------
        public static void Swap<T>(ref T t1, ref T t2)
        {
            T t = t1;
            t1 = t2;
            t2 = t;
        }

        // -----------------
        public static int IntParseAllowEmpty(string strVal)
        {
            if (string.IsNullOrEmpty(strVal))
            {
                return 0;
            }
            else
            {
                return int.Parse(strVal);
            }
        }
        public static double DoubleParseAllowEmpty(string strVal)
        {
            if (string.IsNullOrEmpty(strVal))
            {
                return 0.0;
            }
            else
            {
                return double.Parse(strVal);
            }
        }

        public static T FindInList<T>(IList<T> list, Func<T, bool> testFunc)
        {
            foreach (T item in list)
            {
                if (testFunc(item))
                {
                    return item;
                }
            }
            return default(T);
        }

    }
}
