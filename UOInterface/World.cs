using System;
using System.Collections.Generic;
using System.Linq;

namespace UOInterface
{
    public static partial class World
    {
        private static readonly HashSet<Item> toAdd = new HashSet<Item>();
        private static Serial[] party = new Serial[10];
        private static byte updateRange = 18;

        public static EntityCollection<Item> Items { get; private set; }
        public static EntityCollection<Mobile> Mobiles { get; private set; }
        public static IEnumerable<Item> Ground { get { return Items.Where(item => item.OnGround); } }

        public static IEnumerable<Serial> Party { get { return party.Where(s => s.IsValid); } }
        public static PlayerMobile Player { get; private set; }
        public static Map Map { get; private set; }

        public static event EventHandler MapChanged, Cleared;

        static World()
        {
            Items = new EntityCollection<Item>();
            Mobiles = new EntityCollection<Mobile>();
        }

        [OnInit]
        internal static void Init()
        {
            Client.Disconnecting += (s, e) => Clear();
            InitHandlers();
        }

        private static void Clear()
        {
            Player = null;
            Items.Clear();
            Mobiles.Clear();
            party = new Serial[10];
            movementQueue.Clear();
            Cleared.Raise();
        }

        public static bool IsInParty(Serial serial) { return Array.IndexOf(party, serial) != -1; }
        public static bool Contains(Serial serial)
        {
            if (serial.IsItem)
                return Items.Contains(serial);
            if (serial.IsMobile)
                return Mobiles.Contains(serial);
            return false;
        }

        public static Entity Get(Serial serial)
        {
            if (serial.IsItem)
                return Items.Get(serial);
            if (serial.IsMobile)
                return Mobiles.Get(serial);
            return null;
        }

        private static Item GetOrCreateItem(Serial serial) { return Items.Get(serial) ?? new Item(serial); }
        private static Mobile GetOrCreateMobile(Serial serial) { return Mobiles.Get(serial) ?? new Mobile(serial); }

        private static bool RemoveItem(Serial serial)
        {
            Item item = Items.Remove(serial);
            if (item == null)
            {
                toAdd.RemoveWhere(i => i == serial);
                return false;
            }

            foreach (Item i in item.Items)
                RemoveItem(i);
            item.Items.Clear();
            return true;
        }

        private static bool RemoveMobile(Serial serial)
        {
            Mobile mobile = Mobiles.Remove(serial);
            if (mobile == null)
                return false;

            foreach (Item i in mobile.Items)
                RemoveItem(i);
            mobile.Items.Clear();
            return true;
        }
    }
}