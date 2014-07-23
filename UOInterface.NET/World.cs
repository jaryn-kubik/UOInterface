using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace UOInterface
{
    public static partial class World
    {
        private static readonly ConcurrentDictionary<Serial, Item> items = new ConcurrentDictionary<Serial, Item>(1, 512);
        private static readonly ConcurrentDictionary<Serial, Item> ground = new ConcurrentDictionary<Serial, Item>(1, 512);
        private static readonly ConcurrentDictionary<Serial, Mobile> mobiles = new ConcurrentDictionary<Serial, Mobile>(1, 128);
        private static readonly ConcurrentQueue<Entity> toProcess = new ConcurrentQueue<Entity>();
        private static readonly HashSet<Item> toUpdate = new HashSet<Item>();
        private static readonly List<Serial> party = new List<Serial>();
        private static List<Item> itemsToAdd = new List<Item>(64), itemsRemoved = new List<Item>(64);
        private static List<Mobile> mobilesToAdd = new List<Mobile>(32), mobilesRemoved = new List<Mobile>(32);

        public static IEnumerable<Mobile> Mobiles { get { return mobiles.Select(mobile => mobile.Value); } }
        public static IEnumerable<Item> Items { get { return items.Select(item => item.Value); } }
        public static IEnumerable<Item> Ground { get { return ground.Select(item => item.Value); } }

        public static Serial[] Party { get { lock (party) return party.ToArray(); } }
        public static PlayerMobile Player { get; private set; }
        public static Map Map { get; private set; }

        public static event EventHandler<CollectionChangedEventArgs<Item>> ItemsChanged;
        public static event EventHandler<CollectionChangedEventArgs<Mobile>> MobilesChanged;
        public static event EventHandler MapChanged, Cleared;

        static World()
        {
            Client.Disconnecting += (s, e) => Clear();
            Player = PlayerMobile.Invalid;
            InitHandlers();
        }

        private static void Clear()
        {
            Player = PlayerMobile.Invalid;
            lock (party)
                party.Clear();
            items.Clear();
            ground.Clear();
            mobiles.Clear();
            movementQueue.Clear();
            Cleared.Raise();
        }

        public static bool IsInParty(Serial serial) { lock (party) return party.Contains(serial); }
        public static bool ContainsItem(Serial serial) { return items.ContainsKey(serial); }
        public static bool ContainsMobile(Serial serial) { return mobiles.ContainsKey(serial); }
        public static bool Contains(Serial serial)
        {
            if (serial.IsItem)
                return ContainsItem(serial);
            if (serial.IsMobile)
                return ContainsMobile(serial);
            return false;
        }

        public static Entity GetEntity(Serial serial)
        {
            if (serial.IsItem)
                return GetItem(serial);
            if (serial.IsMobile)
                return GetMobile(serial);
            return null;
        }

        public static Item GetItem(Serial serial)
        {
            Item item;
            return items.TryGetValue(serial, out item) || ground.TryGetValue(serial, out item) ? item : null;
        }

        public static Mobile GetMobile(Serial serial)
        {
            Mobile mobile;
            return mobiles.TryGetValue(serial, out mobile) ? mobile : null;
        }

        private static Item GetOrCreateItem(Serial serial)
        {
            Item item = GetItem(serial);
            if (item == null)
                itemsToAdd.Add(item = new Item(serial));
            return item;
        }

        private static Mobile GetOrCreateMobile(Serial serial)
        {
            Mobile mobile = GetMobile(serial);
            if (mobile == null)
                mobilesToAdd.Add(mobile = new Mobile(serial));
            return mobile;
        }

        private static void RemoveItem(Serial serial)
        {
            Item item;
            if (!items.TryRemove(serial, out item) && !ground.TryRemove(serial, out item))
                return;

            itemsRemoved.Add(item);
            foreach (Item i in item.Items)
                RemoveItem(i);
            item.Clear();
            toProcess.Enqueue(item);
        }

        private static void RemoveMobile(Serial serial)
        {
            Mobile mobile;
            if (!mobiles.TryRemove(serial, out mobile))
                return;

            mobilesRemoved.Add(mobile);
            foreach (Item i in mobile.Items)
                RemoveItem(i);
            mobile.Clear();
            toProcess.Enqueue(mobile);
        }

        private static void ProcessDelta()
        {
            toUpdate.ExceptWith(itemsRemoved);
            foreach (IGrouping<Serial, Item> group in itemsRemoved.GroupBy(i => i.Container))
            {
                Entity container = GetEntity(group.Key);
                if (container != null)
                {
                    foreach (Item i in group)
                        container.RemoveItem(i);
                    toProcess.Enqueue(container);
                }
            }

            CollectionChangedEventArgs<Item> itemsChanged = null;
            CollectionChangedEventArgs<Mobile> mobilesChanged = null;

            if (itemsToAdd.Count > 0 || itemsRemoved.Count > 0)
            {
                foreach (Item item in itemsToAdd)
                    (item.Container.IsValid ? items : ground).TryAdd(item.Serial, item);

                itemsChanged = new CollectionChangedEventArgs<Item>(itemsToAdd, itemsRemoved);
                itemsToAdd = new List<Item>(64);
                itemsRemoved = new List<Item>(64);
            }

            if (mobilesToAdd.Count > 0 || mobilesRemoved.Count > 0)
            {
                foreach (Mobile mobile in mobilesToAdd)
                    mobiles.TryAdd(mobile.Serial, mobile);

                mobilesChanged = new CollectionChangedEventArgs<Mobile>(mobilesToAdd, mobilesRemoved);
                mobilesToAdd = new List<Mobile>(32);
                mobilesRemoved = new List<Mobile>(32);
            }

            foreach (IGrouping<Serial, Item> group in toUpdate.GroupBy(i => i.Container))
            {
                Entity container = GetEntity(group.Key);
                if (container != null)
                {
                    foreach (Item i in group)
                    {
                        container.AddItem(i);
                        toUpdate.Remove(i);
                    }
                    toProcess.Enqueue(container);
                }
            }

            Task.Run(() =>
            {
                try
                {
                    Entity entity;
                    while (toProcess.TryDequeue(out entity))
                        entity.ProcessDelta();

                    if (itemsChanged != null)
                        ItemsChanged.Raise(itemsChanged);
                    if (mobilesChanged != null)
                        MobilesChanged.Raise(mobilesChanged);
                }
                catch (Exception ex) { Client.OnException(ex); }
            });
        }
    }
}