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
            ushort graphic = (ushort)(p.ReadUShort() & 0x3FFF);
            ushort amount = 1;

            if ((serial & 0x80000000) != 0)
                amount = p.ReadUShort();

            if ((graphic & 0x8000) != 0)
                graphic = (ushort)(graphic & 0x7FFF + p.ReadSByte());
            else
                graphic = (ushort)(graphic & 0x7FFF);

            ushort x = p.ReadUShort();
            ushort y = p.ReadUShort();
            Direction dir = 0;

            if ((x & 0x8000) != 0)
                dir = (Direction)p.ReadByte();//wtf???

            sbyte z = p.ReadSByte();
            ushort hue = 0;
            if ((y & 0x8000) != 0)
                hue = p.ReadUShort();

            UOFlags flags = 0;
            if ((y & 0x4000) != 0)
                flags = (UOFlags)p.ReadByte();

            item.OnAppearanceChanged(graphic, hue);
            item.OnAttributesChanged(flags, amount);
            item.OnMoved(new Position((ushort)(x & 0x7FFF), (ushort)(y & 0x3FFF), z), dir);
            item.OnOwnerChanged(Serial.Invalid);
            AddItem(item);
        }

        private static void OnContainerContentUpdate(Packet p)//0x25
        {
            Item item = GetOrCreateItem(p.ReadUInt());
            ushort graphic = (ushort)(p.ReadUShort() + p.ReadSByte());

            item.OnAttributesChanged(amount: Math.Max(p.ReadUShort(), (ushort)1));
            item.OnMoved(new Position(p.ReadUShort(), p.ReadUShort()));

            if (usePostKRPackets.Value)
                p.ReadByte(); //useless?

            item.OnOwnerChanged(p.ReadUInt());
            item.OnAppearanceChanged(graphic, p.ReadUShort());
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
            ushort graphic = (ushort)(p.ReadUShort() + p.ReadSByte());
            Layer layer = (Layer)p.ReadByte();
            item.OnOwnerChanged(p.ReadUInt(), layer);
            item.OnAppearanceChanged(graphic, p.ReadUShort());
            AddItem(item);
        }
    }
}