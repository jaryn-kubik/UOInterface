using System;
using System.Collections.Generic;
using UOInterface.Network;

namespace UOInterface
{
    public static partial class World
    {
        //sometimes child items are sent before parent containers...
        //no idea why, they like to make things complicated i guess
        private static void ReadContainerContent(Packet p)
        {
            Item item = GetOrCreateItem(p.ReadUInt());
            item.Graphic = (ushort)(p.ReadUShort() + p.ReadSByte());
            item.Amount = Math.Max(p.ReadUShort(), (ushort)1);
            item.Position = new Position(p.ReadUShort(), p.ReadUShort());
            if (usePostKRPackets.Value)
                p.ReadByte(); //gridnumber - useless?

            item.Container = p.ReadUInt();
            item.Hue = p.ReadUShort();
            toUpdate.Add(item);
            toProcess.Enqueue(item);
        }

        private static void OnContainerContentUpdate(Packet p)//0x25
        {
            ReadContainerContent(p);
            ProcessDelta();
        }

        private static void OnContainerContent(Packet p)//0x3C
        {
            ushort count = p.ReadUShort();
            for (int i = 0; i < count; i++)
                ReadContainerContent(p);
            ProcessDelta();
        }

        private static void OnEquipUpdate(Packet p)//0x2E
        {
            Item item = GetOrCreateItem(p.ReadUInt());
            item.Graphic = (ushort)(p.ReadUShort() + p.ReadSByte());
            item.Layer = (Layer)p.ReadByte();
            item.Container = p.ReadUInt();
            item.Hue = p.ReadUShort();
            toUpdate.Add(item);
            toProcess.Enqueue(item);
            ProcessDelta();
        }

        private static void OnWorldItem(Packet p)//0x1A
        {
            uint serial = p.ReadUInt();
            Item item = GetOrCreateItem(serial & 0x7FFFFFFF);

            ushort graphic = (ushort)(p.ReadUShort() & 0x3FFF);
            item.Amount = (serial & 0x80000000) != 0 ? p.ReadUShort() : (ushort)1;

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

            item.Container = Serial.Invalid;
            toProcess.Enqueue(item);
            ProcessDelta();
        }

        private static void OnWorldItemSA(Packet p)//0xF3
        {
            p.Skip(2);//unknown

            byte type = p.ReadByte();
            Item item = GetOrCreateItem(p.ReadUInt());

            ushort g = p.ReadUShort();
            if (type == 2)
                g |= 0x4000;
            item.Graphic = g;
            item.Direction = (Direction)p.ReadByte();

            item.Amount = p.ReadUShort();
            p.Skip(2);//amount again? wtf???

            item.Position = new Position(p.ReadUShort(), p.ReadUShort(), p.ReadSByte());
            p.Skip(1);//light? wtf?

            item.Hue = p.ReadUShort();
            item.Flags = (UOFlags)p.ReadByte();

            if (usePostHSChanges.Value)
                p.ReadUShort();//unknown

            item.Container = Serial.Invalid;
            toProcess.Enqueue(item);
            ProcessDelta();
        }

        private static void OnProperties(Packet p) //0xD6
        {
            p.Skip(2);
            Entity entity = GetEntity(p.ReadUInt());
            if (entity == null)
                return;
            p.Skip(6);
            entity.UpdateProperties(ReadProperties(p));
            toProcess.Enqueue(entity);
            ProcessDelta();
        }

        private static IEnumerable<Entity.UOProperty> ReadProperties(Packet p)
        {
            uint cliloc;
            while ((cliloc = p.ReadUInt()) != 0)
            {
                ushort len = p.ReadUShort();
                string str = p.ReadUnicodeReversed(len);
                yield return new Entity.UOProperty(cliloc, str);
            }
        }
    }
}