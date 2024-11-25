/*
 * This file is part of the CatLib package.
 *
 * (c) CatLib <support@catlib.io>
 *
 * For the full copyright and license information, please view the LICENSE
 * file that was distributed with this source code.
 *
 * Document: https://catlib.io/
 */


using System;
using System.Collections.Generic;

namespace My.EventDispatcher
{
    public class MyEventDispatcher
    {
        private static Dictionary<string, List<EventHandler> > listeners = new();
        
        public static bool AddListener(string eventName, EventHandler handler)
        {
            if (string.IsNullOrEmpty(eventName) || handler == null)
            {
                return false;
            }

            if (!listeners.TryGetValue(eventName, out List<EventHandler> handlers))
            {
                listeners[eventName] = handlers = new List<EventHandler>();
            }
            else if (handlers.Contains(handler))
            {
                return false;
            }

            handlers.Add(handler);
            return true;
        }
        
        public static bool RemoveListener(string eventName, EventHandler handler = null)
        {
            if (handler == null)
            {
                return listeners.Remove(eventName);
            }

            if (!listeners.TryGetValue(eventName, out List<EventHandler> handlers))
            {
                return false;
            }

            var status = handlers.Remove(handler);
            if (handlers.Count <= 0)
            {
                listeners.Remove(eventName);
            }

            return status;
        }
        

        public static void RaiseMessage(string eventName, object sender = null, EventArgs e = null)
        {
            e ??= EventArgs.Empty;
            if (listeners.TryGetValue(eventName, out List<EventHandler> handlers))
            {
                foreach (var listener in handlers)
                {
                    listener(sender, e);
                }
            }
        }
    }
}
