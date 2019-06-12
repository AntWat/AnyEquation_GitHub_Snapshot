using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using AnyEquation.Equations.Common;
using Xamarin.Forms;

/* TODO: 
 * Replace with a better message bus, e.g. MVVM Light.  
 * Weak references?
*/

namespace AnyEquation.Common
{
    public class AntMessenger<TMessage>
    {
        private static AntMessenger<TMessage> _defaultMessenger = new AntMessenger<TMessage>();
        public static AntMessenger<TMessage> Default()
        {
            return _defaultMessenger;
        }

        class ActionInfo
        {
            public ActionInfo(Action<TMessage> action, bool invokeOnUiThread)
            {
                Action = action;
                InvokeOnUiThread = invokeOnUiThread;
            }
            public Action<TMessage> Action { get; set; }
            public bool InvokeOnUiThread { get; set; }
        }

        // Note that we use WeakReference so that the MessageBus cannot end up being the only thing stopping an object being garbage collected.
        private IDictionary<WeakReference, IDictionary<WeakReference, ActionInfo>> _objectsAndActions = new Dictionary<WeakReference, IDictionary<WeakReference, ActionInfo>>();
        private IDictionary<WeakReference, IDictionary<WeakReference, ActionInfo>> ObjectsAndActions
        {
            get { return _objectsAndActions; }
            set { _objectsAndActions = value; }
        }

        private IDictionary<WeakReference, ActionInfo> FindActions(object cntxt)
        {
            foreach (var item in ObjectsAndActions)
            {
                if (item.Key.Target == cntxt)
                {
                    return item.Value;
                }
            }
            return null;
        }

        public virtual void Register(object recipient, Action<TMessage> action, bool invokeOnUiThread, object context=null)
        {
            try
            {
                ActionInfo actionInfo = new ActionInfo(action, invokeOnUiThread);

                object cntxt = context;
                if (cntxt == null) cntxt = _defaultMessenger;

                IDictionary<WeakReference, ActionInfo> actions = FindActions(cntxt);
                if (actions==null) 
                {
                    actions = new Dictionary<WeakReference, ActionInfo>();
                    ObjectsAndActions.Add(new WeakReference(cntxt), actions);
                }

                foreach (var item in actions)
                {
                    if (item.Key.Target == recipient)
                    {
                        actions.Remove(item);   // Should only be one, so exit loop...
                        break;
                    }
                }
                actions.Add(new WeakReference(recipient), actionInfo);
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }

        public virtual void Send(TMessage message, object context=null)
        {
            try
            {
                object cntxt = context;
                if (cntxt == null) cntxt = _defaultMessenger;

                IDictionary<WeakReference, ActionInfo> actions = FindActions(cntxt);
                if (actions != null)
                {
                    foreach (var item in actions)
                    {
                        object recipient = item.Key.Target;
                        if (recipient!=null)  // Otherwise the recipient has died and been garbage collected
                        {
                            ActionInfo actionInfo = item.Value;
                            if (actionInfo.InvokeOnUiThread)
                            {
                                Device.BeginInvokeOnMainThread(() => {
                                    actionInfo.Action(message);
                                });
                            }
                            else
                            {
                                actionInfo.Action(message);
                            }
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }

        public virtual void UnRegister(object recipient, object context=null)
        {
            try
            {
                object cntxt = context;
                if (cntxt == null) cntxt = _defaultMessenger;

                IDictionary<WeakReference, ActionInfo> actions = FindActions(cntxt);
                if (actions!=null)
                {
                    foreach (var item in actions)
                    {
                        if (item.Key.Target == recipient)
                        {
                            actions.Remove(item);   // Should only be one, so exit loop...
                            break;
                        }
                    }
                }

            }
            catch (Exception ex)
            {
                Logging.LogException(ex);
                throw;
            }
        }
    }
}
