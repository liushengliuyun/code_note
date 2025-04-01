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
using System.Linq;

namespace My.EventDispatcher
{
 public class MyEventDispatcher : IDisposable
    {
        private readonly IDictionary<string, IList<EventHandler>> listeners;

        private MyEventDispatcher parent;

        private List<MyEventDispatcher> children;

        private List<MyEventDispatcher> Children
        {
            get { return children ??= new List<MyEventDispatcher>(); }
        }

        public void SetParent(MyEventDispatcher parent)
        {
            this.parent = parent;
            parent.Children.Add(this);
        }

        public static MyEventDispatcher Root = new MyEventDispatcher();

        /// <summary>
        /// Initializes a new instance of the <see cref="MyEventDispatcher"/> class.
        /// </summary>
        private MyEventDispatcher()
        {
            listeners = new Dictionary<string, IList<EventHandler>>();
        }

        private MyEventDispatcher(MyEventDispatcher parent)
        {
            listeners = new Dictionary<string, IList<EventHandler>>();
            SetParent(parent);
        }

        public static MyEventDispatcher New()
        {
            return new MyEventDispatcher(Root);
        }
        
        /// <inheritdoc />
        public virtual bool AddListener(string eventName, EventHandler handler)
        {
            if (string.IsNullOrEmpty(eventName) || handler == null)
            {
                return false;
            }

            if (!listeners.TryGetValue(eventName, out IList<EventHandler> handlers))
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

        /// <inheritdoc />
        public virtual void Raise(string eventName, object sender = null, EventArgs e = null)
        {
            // Guard.Requires<LogicException>(!(sender is EventArgs),
            //     $"Passed event args for the parameter {sender}, Did you make a wrong method call?");

            e ??= EventArgs.Empty;
            if (listeners.TryGetValue(eventName, out IList<EventHandler> handlers))
            {
                for (int i = 0; i < handlers.Count; i++)
                {
                    handlers[i](sender, e);
                }
            }

            ProcessRemoveList();

            if (children == null || !children.Any()) return;

            for (int i = 0; i < children.Count; i++)
            {
                children[i].Raise(eventName, sender, e);
            }

            ProcessRemoveList();
        }


        private void ProcessRemoveList()
        {
            if (removeList != null && children != null)
            {
                foreach (var child in removeList)
                {
                    children.Remove(child);
                }

                removeList.Clear();
            }
        }

        /// <inheritdoc />
        public virtual EventHandler[] GetListeners(string eventName)
        {
            if (!listeners.TryGetValue(eventName, out IList<EventHandler> handlers))
            {
                return Array.Empty<EventHandler>();
            }

            return handlers.ToArray();
        }

        /// <inheritdoc />
        public virtual bool HasListener(string eventName)
        {
            return listeners.ContainsKey(eventName);
        }

        /// <inheritdoc />
        public virtual bool RemoveListener(string eventName, EventHandler handler = null)
        {
            if (handler == null)
            {
                return listeners.Remove(eventName);
            }

            if (!listeners.TryGetValue(eventName, out IList<EventHandler> handlers))
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

        private List<MyEventDispatcher> removeList;


        public void AddRemoveList(MyEventDispatcher toRemove)
        {
            if (removeList == null)
            {
                removeList = new();
            }

            removeList.Add(toRemove);
        }

        public void Dispose()
        {
            listeners.Clear();
            if (parent != null)
            {
                parent.AddRemoveList(this);
            }
        }
    }
}
