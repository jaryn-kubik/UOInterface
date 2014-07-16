using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;

namespace UOInterface
{
    public static partial class World
    {
        private static readonly ConcurrentDictionary<Serial, Item> items = new ConcurrentDictionary<Serial, Item>(1, 512);
        private static readonly ConcurrentDictionary<Serial, Mobile> mobiles = new ConcurrentDictionary<Serial, Mobile>(1, 64);
        private static readonly List<Serial> party = new List<Serial>();

        public static IEnumerable<Item> Items { get { return items.Select(item => item.Value); } }
        public static IEnumerable<Mobile> Mobiles { get { return mobiles.Select(mobile => mobile.Value); } }
        public static Serial[] Party { get { lock (party) return party.ToArray(); } }
        public static Mobile Player { get; private set; }
        public static Map Map { get; private set; }

        public static event EventHandler<Item> ItemAdded;
        public static event EventHandler<Item> ItemRemoved;
        public static event EventHandler<Mobile> MobileAdded;
        public static event EventHandler<Mobile> MobileRemoved;
        public static event EventHandler MapChanged, Cleared;

        static World()
        {
            Client.Disconnecting += (s, e) => Clear();
            Player = Mobile.Invalid;
            InitHandlers();
        }

        private static void Clear()
        {
            Player = Mobile.Invalid;
            lock (party)
                party.Clear();
            items.Clear();
            mobiles.Clear();
            movementQueue.Clear();
            Cleared.RaiseAsync();
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
            return Entity.Invalid;
        }

        public static Item GetItem(Serial serial)
        {
            Item item;
            return items.TryGetValue(serial, out item) ? item : Item.Invalid;
        }

        public static Mobile GetMobile(Serial serial)
        {
            Mobile mobile;
            return mobiles.TryGetValue(serial, out mobile) ? mobile : Mobile.Invalid;
        }

        private static Item GetOrCreateItem(Serial serial)
        {
            Item item;
            return items.TryGetValue(serial, out item) ? item : new Item(serial);
        }

        private static Mobile GetOrCreateMobile(Serial serial)
        {
            Mobile mobile;
            return mobiles.TryGetValue(serial, out mobile) ? mobile : new Mobile(serial);
        }

        private static void AddItem(Item item)
        {
            if (items.TryAdd(item.Serial, item))
                ItemAdded.RaiseAsync(item);
        }

        private static void AddMobile(Mobile mobile)
        {
            if (mobiles.TryAdd(mobile.Serial, mobile))
                MobileAdded.RaiseAsync(mobile);
        }

        private static void Remove(Serial serial)
        {
            if (serial.IsItem)
            {
                Item item;
                if (items.TryRemove(serial, out item))
                    ItemRemoved.RaiseAsync(item);
            }
            else if (serial.IsMobile)
            {
                Mobile mobile;
                if (mobiles.TryRemove(serial, out mobile))
                    MobileRemoved.RaiseAsync(mobile);
            }
            else
                throw new ArgumentException("serial");
            foreach (Serial s in GetContainerContents(serial))
                Remove(s);
        }

        private static IEnumerable<Serial> GetContainerContents(Serial serial, bool recursive = true)
        {
            foreach (Item item in Items.Where(item => item.Container == serial))
            {
                if (recursive)
                    foreach (Serial s in GetContainerContents(item.Serial))
                        yield return s;
                yield return item;
            }
        }
    }
}