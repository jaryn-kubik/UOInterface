using System;
using System.Collections.Generic;

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
        public CollectionChangedEventArgs(IReadOnlyList<T> added, IReadOnlyList<T> removed)
        {
            Added = added;
            Removed = removed;
        }

        public IReadOnlyList<T> Added { get; private set; }
        public IReadOnlyList<T> Removed { get; private set; }
    }
}