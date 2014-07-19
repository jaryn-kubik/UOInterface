using System;
using System.Collections.Generic;
using System.Linq;

namespace UOInterface
{
    public static class Extensions
    {
        public static void Raise(this EventHandler handler, object sender = null)
        {
            if (handler != null)
                handler(sender, EventArgs.Empty);
        }

        public static void Raise<T>(this EventHandler<T> handler, T e, object sender = null)
        {
            if (handler != null)
                handler(sender, e);
        }
    }

    public class CollectionChangedEventArgs<T> : EventArgs
    {
        public CollectionChangedEventArgs(IEnumerable<T> items, IEnumerable<T> removed)
        {
            Added = items.ToArray();
            Removed = removed.ToArray();
        }

        public IEnumerable<T> Added { get; private set; }
        public IEnumerable<T> Removed { get; private set; }
    }
}