using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace UOInterface
{
    public class EntityCollection<T> : IEnumerable<T> where T : Entity
    {
        private readonly ConcurrentDictionary<Serial, T> entities = new ConcurrentDictionary<Serial, T>();
        private readonly List<T> added = new List<T>(), removed = new List<T>();

        public event EventHandler<CollectionChangedEventArgs<T>> Added, Removed;
        internal void ProcessDelta()
        {
            if (added.Count > 0)
            {
                var list = new CollectionChangedEventArgs<T>(added);
                added.Clear();
                Added.RaiseAsync(list);
            }

            if (removed.Count > 0)
            {
                var list = new CollectionChangedEventArgs<T>(removed);
                removed.Clear();
                Removed.RaiseAsync(list);
            }
        }

        public bool Contains(Serial serial) { return entities.ContainsKey(serial); }
        public T Get(Serial serial)
        {
            T entity;
            return entities.TryGetValue(serial, out entity) ? entity : null;
        }

        internal bool Add(T entity)
        {
            if (!entities.TryAdd(entity.Serial, entity))
                return false;
            added.Add(entity);
            return true;
        }

        internal T Remove(Serial serial)
        {
            T entity;
            if (entities.TryRemove(serial, out entity))
                removed.Add(entity);
            return entity;
        }

        internal void Clear()
        {
            removed.AddRange(this);
            entities.Clear();
            ProcessDelta();
        }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        public IEnumerator<T> GetEnumerator() { return entities.Select(e => e.Value).GetEnumerator(); }
    }

    public class CollectionChangedEventArgs<T> : EventArgs, IEnumerable<T>
    {
        private readonly IReadOnlyList<T> data;
        public CollectionChangedEventArgs(IEnumerable<T> list) { data = list.ToArray(); }
        public int Count { get { return data.Count; } }

        IEnumerator IEnumerable.GetEnumerator() { return GetEnumerator(); }
        public IEnumerator<T> GetEnumerator() { return data.GetEnumerator(); }
    }
}