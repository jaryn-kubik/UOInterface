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

        private readonly SortedSet<Handler>[] handlers = new SortedSet<Handler>[0x100];
        private Handlers()
        {
            for (int i = 0; i < handlers.Length; i++)
                handlers[i] = new SortedSet<Handler>();
        }

        public void Add(byte id, Action<Packet> handler, Priority priority = Priority.Normal)
        {
            lock (handlers)
                handlers[id].Add(new Handler(handler, priority));
        }

        public void Remove(byte id, Action<Packet> handler)
        {
            lock (handlers)
                handlers[id].RemoveWhere(p => p.Callback == handler);
        }

        private void OnPacket(object sender, Packet p)
        {
            lock (handlers)
                foreach (Handler handler in handlers[p.Id])
                {
                    p.MoveToData();
                    handler.Callback(p);
                }
        }

        private class Handler : IComparable<Handler>
        {
            private readonly Priority priority;
            public Action<Packet> Callback { get; private set; }

            public Handler(Action<Packet> callback, Priority priority)
            {
                this.priority = priority;
                Callback = callback;
            }

            public int CompareTo(Handler other)
            {
                if (Callback == other.Callback)
                    return 0;
                int res = priority.CompareTo(other.priority);
                return res == 0 ? 1 : res;
            }
        }
    }
}