using System;
using UOInterface.Network;

namespace UOInterface
{
    public static partial class World
    {
        private static void OnWorldItem(Packet p)//0x1A
        {
            uint serial = p.ReadUInt();
            Item item = GetOrCreateItem(serial);
            lock (item.SyncRoot)
            {
                ushort graphic = (ushort)(p.ReadUShort() & 0x3FFF);

                if ((serial & 0x80000000) != 0)
                    item.Amount = p.ReadUShort();
                else
                    item.Amount = 1;

                if ((graphic & 0x8000) != 0)
                    item.Graphic = (ushort)(graphic & 0x7FFF + p.ReadSByte());
                else
                    item.Graphic = (ushort)(graphic & 0x7FFF);

                ushort x = p.ReadUShort();
                ushort y = p.ReadUShort();

                if ((x & 0x8000) != 0)
                    item.Direction = (Direction)p.ReadByte();//wtf???

                item.Position = new Position((ushort)(x & 0x7FFF), (ushort)(y & 0x3FFF), p.ReadSByte());

                if ((y & 0x8000) != 0)
                    item.Hue = p.ReadUShort();

                if ((y & 0x4000) != 0)
                    item.Flags = (UOFlags)p.ReadByte();
            }
            item.ProcessDelta();
            AddItem(item);
        }

        private static void OnContainerContentUpdate(Packet p)//0x25
        {
            Item item = GetOrCreateItem(p.ReadUInt());
            lock (item.SyncRoot)
            {
                item.Graphic = (ushort)(p.ReadUShort() + p.ReadSByte());
                item.Amount = Math.Max(p.ReadUShort(), (ushort)1);
                item.Position = new Position(p.ReadUShort(), p.ReadUShort());
                if (usePostKRPackets.Value)
                    p.ReadByte(); //useless?

                item.Container = p.ReadUInt();
                item.Hue = p.ReadUShort();
            }
            item.ProcessDelta();
            AddItem(item);
        }

        private static void OnContainerContent(Packet p)//0x3C
        {
            ushort count = p.ReadUShort();
            for (int i = 0; i < count; i++)
                OnContainerContentUpdate(p);
        }

        private static void OnEquipUpdate(Packet p)//0x2E
        {
            Item item = GetOrCreateItem(p.ReadUInt());
            lock (item.SyncRoot)
            {
                item.Graphic = (ushort)(p.ReadUShort() + p.ReadSByte());
                item.Layer = (Layer)p.ReadByte();
                item.Container = p.ReadUInt();
                item.Hue = p.ReadUShort();

                Mobile m = GetMobile(item.Container);
                if (m.IsValid)
                {
                    m[item.Layer] = item;
                    m.ProcessDelta();
                }
            }
            item.ProcessDelta();
            AddItem(item);
        }
    }
}