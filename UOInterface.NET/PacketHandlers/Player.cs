using System.Linq;
using UOInterface.Network;

namespace UOInterface
{
    public static partial class World
    {
        private static void OnLoginConfirm(Packet p)//0x1B
        {
            Clear();
            Player = GetOrCreateMobile(p.ReadUInt());
            p.Skip(4);//unknown
            Player.Graphic = p.ReadUShort();
            ushort x = p.ReadUShort();
            ushort y = p.ReadUShort();
            sbyte z = (sbyte)p.ReadUShort();
            Player.Direction = (Direction)p.ReadByte() & ~Direction.Running;
            //p.Skip(9);
            //p.ReadUShort();//map width
            //p.ReadUShort();//map height

            Player.Position = new Position(x, y, z);
            AddMobile(Player);
        }

        private static void OnPlayerUpdate(Packet p)//0x20
        {
            p.Skip(4);
            movementQueue.Clear();
            Player.Graphic = p.ReadUShort();
            p.Skip(1);//unknown
            Player.Hue = p.ReadUShort();
            Player.Flags = (UOFlags)p.ReadByte();
            ushort x = p.ReadUShort();
            ushort y = p.ReadUShort();
            p.Skip(2);//unknown
            Player.Direction = (Direction)p.ReadByte() & ~Direction.Running;
            sbyte z = p.ReadSByte();

            OnPlayerMoved();
            Player.Position = new Position(x, y, z);
            AddMobile(Player);
        }

        private static void OnWarMode(Packet packet)//0x72
        { Player.WarMode = packet.ReadBool(); }

        private static void OnPlayerMoved()
        {
            foreach (Mobile m in Mobiles.Where(m => m.Distance > 0x20 && !party.Contains(m)))
                Remove(m);
            foreach (Item i in Items.Where(i => i.OnGround && i.Distance > 0x20))
                Remove(i);
        }
    }
}