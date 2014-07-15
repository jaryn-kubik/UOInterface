using System;
using System.Collections.Generic;

namespace UOInterface.Network
{
    public enum Priority { Highest, High, Normal, Low, Lowest }
    public static class Communication
    {
        private static readonly SortedSet<PacketHandler>[] toClient = new SortedSet<PacketHandler>[0x100];
        private static readonly SortedSet<PacketHandler>[] toServer = new SortedSet<PacketHandler>[0x100];

        static Communication()
        {
            Client.PacketToClient += Client_PacketToClient;
            Client.PacketToServer += Client_PacketToServer;
        }

        public static void AddToClientHandler(byte id, Action<Packet> handler, Priority priority = Priority.Normal)
        {
            lock (toClient)
            {
                var set = toClient[id] ?? (toClient[id] = new SortedSet<PacketHandler>());
                set.Add(new PacketHandler(handler, priority));
            }
        }

        public static void AddToServerHandler(byte id, Action<Packet> handler, Priority priority = Priority.Normal)
        {
            lock (toServer)
            {
                var set = toServer[id] ?? (toServer[id] = new SortedSet<PacketHandler>());
                set.Add(new PacketHandler(handler, priority));
            }
        }

        private static void Client_PacketToClient(object sender, Packet p)
        {
            try
            {
                lock (toClient)
                    foreach (PacketHandler handler in toClient[p.ID])
                        handler.Handler(p);
            }
            catch (Exception) { throw new NotImplementedException(); }
        }

        private static void Client_PacketToServer(object sender, Packet p)
        {
            try
            {
                lock (toServer)
                    foreach (PacketHandler handler in toServer[p.ID])
                        handler.Handler(p);
            }
            catch (Exception) { throw new NotImplementedException(); }
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