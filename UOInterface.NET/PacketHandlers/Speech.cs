using UOInterface.Network;

namespace UOInterface
{
    public static partial class World
    {
        private static void OnAsciiMessage(Packet p)//0x1C
        {
            Mobile mobile = GetMobile(p.ReadUInt());
            if (!mobile.IsValid)//server?
                return;
            ushort graphic = p.ReadUShort();
            p.Skip(5);
            mobile.OnAppearanceChanged(graphic, name: p.ReadStringAscii(30));
            AddMobile(mobile);
        }

        private static void OnUnicodeMessage(Packet p)//0xAE
        {
            Mobile mobile = GetMobile(p.ReadUInt());
            if (!mobile.IsValid)//server?
                return;
            ushort graphic = p.ReadUShort();
            p.Skip(9);
            mobile.OnAppearanceChanged(graphic, name: p.ReadStringAscii(30));
            AddMobile(mobile);
        }

        private static void OnLocalizedMessage(Packet p)//0xC1
        {
            Mobile mobile = GetMobile(p.ReadUInt());
            if (!mobile.IsValid)//server?
                return;
            ushort graphic = p.ReadUShort();
            p.Skip(9);
            mobile.OnAppearanceChanged(graphic, name: p.ReadStringAscii(30));
            AddMobile(mobile);
        }
    }
}