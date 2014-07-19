using UOInterface.Network;

namespace UOInterface
{
    public static partial class World
    {
        private static void OnAsciiMessage(Packet p)//0x1C
        {
            Mobile mobile = GetMobile(p.ReadUInt());
            if (!mobile.IsValid)
                return;
            lock (mobile.SyncRoot)
            {
                mobile.Graphic = p.ReadUShort();
                p.Skip(5);
                mobile.Name = p.ReadStringAscii(30);
            }
            mobile.ProcessDelta();
        }

        private static void OnUnicodeMessage(Packet p)//0xAE
        {
            Mobile mobile = GetMobile(p.ReadUInt());
            if (!mobile.IsValid)
                return;
            lock (mobile.SyncRoot)
            {
                mobile.Graphic = p.ReadUShort();
                p.Skip(9);
                mobile.Name = p.ReadStringAscii(30);
            }
            mobile.ProcessDelta();
        }

        private static void OnLocalizedMessage(Packet p)//0xC1
        {
            Mobile mobile = GetMobile(p.ReadUInt());
            if (!mobile.IsValid)
                return;
            lock (mobile.SyncRoot)
            {
                mobile.Graphic = p.ReadUShort();
                p.Skip(9);
                mobile.Name = p.ReadStringAscii(30);
            }
            mobile.ProcessDelta();
        }

        private static void OnLocalizedMessageAffix(Packet p)//0xCC
        {
            Mobile mobile = GetMobile(p.ReadUInt());
            if (!mobile.IsValid)
                return;
            lock (mobile.SyncRoot)
            {
                mobile.Graphic = p.ReadUShort();
                p.Skip(10);
                mobile.Name = p.ReadStringAscii(30);
            }
            mobile.ProcessDelta();
        }
    }
}