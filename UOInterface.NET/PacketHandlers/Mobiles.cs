using UOInterface.Network;

namespace UOInterface
{
    public static partial class World
    {
        private static void OnMobileMoving(Packet p)//0x77
        {
            Mobile mobile = GetOrCreateMobile(p.ReadUInt());
            mobile.Graphic = p.ReadUShort();
            ushort x = p.ReadUShort();
            ushort y = p.ReadUShort();
            sbyte z = p.ReadSByte();
            mobile.Direction = (Direction)p.ReadByte() & ~Direction.Running;
            mobile.Hue = p.ReadUShort();
            mobile.Flags = (UOFlags)p.ReadByte();
            mobile.Notoriety = (Notoriety)p.ReadByte();

            mobile.Position = new Position(x, y, z);
            AddMobile(mobile);
        }

        private static void OnMobileIncoming(Packet p)//0x78
        {
            Mobile mobile = GetOrCreateMobile(p.ReadUInt());
            mobile.Graphic = p.ReadUShort();
            ushort x = p.ReadUShort();
            ushort y = p.ReadUShort();
            sbyte z = p.ReadSByte();
            mobile.Direction = (Direction)p.ReadByte() & ~Direction.Running;
            mobile.Hue = p.ReadUShort();
            mobile.Flags = (UOFlags)p.ReadByte();
            mobile.Notoriety = (Notoriety)p.ReadByte();

            uint itemSerial;
            while ((itemSerial = p.ReadUInt()) != 0)
            {
                Item item = GetOrCreateItem(itemSerial);
                ushort graphic = p.ReadUShort();

                if (useNewMobileIncoming)
                    item.Graphic = graphic;
                else if (usePostSAChanges)
                    item.Graphic = (ushort)(graphic & 0x7FFF);
                else
                    item.Graphic = (ushort)(graphic & 0x3FFF);

                item.Layer = (Layer)p.ReadByte();

                if (useNewMobileIncoming || (graphic & 0x8000) != 0)
                    item.Hue = p.ReadUShort();

                item.Container = mobile.Serial;
                mobile[item.Layer] = item.Serial;
                AddItem(item);
            }

            mobile.Position = new Position(x, y, z);
            AddMobile(mobile);
        }
    }
}