using System;
using System.Linq;
using UOInterface.Network;

namespace UOInterface
{
    public static partial class World
    {
        private static void OnLoginConfirm(Packet p)//0x1B
        {
            Clear();
            Player = new PlayerMobile(p.ReadUInt());
            p.Skip(4);//unknown

            ushort graphic = p.ReadUShort();
            Player.OnMoved(new Position(p.ReadUShort(), p.ReadUShort(), (sbyte)p.ReadUShort()), (Direction)p.ReadByte());
            Player.OnAppearanceChanged(graphic, 0);
            //p.Skip(9);//unknown
            //p.ReadUShort();//map width
            //p.ReadUShort();//map height

            AddMobile(Player);
        }

        private static void OnPlayerUpdate(Packet p)//0x20
        {
            if (p.ReadUInt() != Player)
                throw new Exception("OnMobileStatus");//does this happen?
            movementQueue.Clear();

            Player.OnAppearanceChanged((ushort)(p.ReadUShort() + p.ReadSByte()), p.ReadUShort());
            Player.OnFlagsChanged((MobileFlags)p.ReadByte());
            ushort x = p.ReadUShort();
            ushort y = p.ReadUShort();
            p.Skip(2);//unknown
            Direction dir = (Direction)p.ReadByte();
            sbyte z = p.ReadSByte();

            Player.OnMoved(new Position(x, y, z), dir);
            OnPlayerMoved();
            AddMobile(Player);
        }

        private static void OnWarMode(Packet packet)//0x72
        { Player.OnFlagsChanged(warMode: packet.ReadBool()); }

        private static void OnPlayerMoved()
        {
            foreach (Mobile m in Mobiles.Where(m => m.Distance > 0x20 && !party.Contains(m)))
                Remove(m);
            foreach (Item i in Items.Where(i => i.OnGround && i.Distance > 0x20))
                Remove(i);
        }
    }
}