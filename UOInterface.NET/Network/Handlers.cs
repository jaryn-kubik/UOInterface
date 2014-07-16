using System;
using System.Collections.Generic;

namespace UOInterface.Network
{
    public enum Priority { Highest, High, Normal, Low, Lowest }
    public class Handlers
    {
        public static Handlers ToClient { get; set; }
        public static Handlers ToServer { get; set; }

        static Handlers()
        {
            ToClient = new Handlers();
            ToServer = new Handlers();
            Client.PacketToClient += ToClient.OnPacket;
            Client.PacketToServer += ToServer.OnPacket;
        }

        private readonly SortedSet<PacketHandler>[] handlers = new SortedSet<PacketHandler>[0x100];
        private Handlers() { }

        public void Add(byte id, Action<Packet> handler, Priority priority = Priority.Normal)
        {
            lock (handlers)
            {
                var set = handlers[id] ?? (handlers[id] = new SortedSet<PacketHandler>());
                set.Add(new PacketHandler(handler, priority));
            }
        }

        public void Remove(byte id, Action<Packet> handler)
        {
            lock (handlers)
                if (handlers[id] != null)
                    handlers[id].RemoveWhere(p => p.Handler == handler);
        }

        private void OnPacket(object sender, Packet p)
        {
            //try
            //{
            lock (handlers)
                if (handlers[p.ID] != null)
                    foreach (PacketHandler handler in handlers[p.ID])
                        handler.Handler(p);
            //}
            //catch (Exception ex) { Exception.Raise(ex); }
        }

        private class PacketHandler : IComparable<PacketHandler>
        {
            private readonly Priority priority;
            public Action<Packet> Handler { get; private set; }

            public PacketHandler(Action<Packet> handler, Priority priority)
            {
                this.priority = priority;
                Handler = handler;
            }

            public int CompareTo(PacketHandler other) { return priority.CompareTo(other.priority); }
        }
    }
}