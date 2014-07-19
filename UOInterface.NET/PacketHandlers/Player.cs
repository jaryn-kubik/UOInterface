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
            mobilesToAdd.Add(Player = new PlayerMobile(p.ReadUInt()));
            p.Skip(4);//unknown

            lock (Player.SyncRoot)
            {
                Player.Graphic = p.ReadUShort();
                Player.Position = new Position(p.ReadUShort(), p.ReadUShort(), (sbyte)p.ReadUShort());
                Player.Direction = (Direction)p.ReadByte();
                //p.Skip(9);//unknown
                //p.ReadUShort();//map width
                //p.ReadUShort();//map height
            }
            Player.ProcessDelta();
            ProcessDelta();
        }

        private static void OnPlayerUpdate(Packet p)//0x20
        {
            if (p.ReadUInt() != Player)
                throw new Exception("OnMobileStatus");//does this happen?
            movementQueue.Clear();

            lock (Player.SyncRoot)
            {
                Player.Graphic = (ushort)(p.ReadUShort() + p.ReadSByte());
                Player.Hue = p.ReadUShort();
                Player.Flags = (UOFlags)p.ReadByte();

                ushort x = p.ReadUShort();
                ushort y = p.ReadUShort();
                p.Skip(2);//unknown
                Player.Direction = (Direction)p.ReadByte();
                Player.Position = new Position(x, y, p.ReadSByte());
            }
            OnPlayerMoved();
            Player.ProcessDelta();
        }

        private static void OnWarMode(Packet packet)//0x72
        { Player.WarMode = packet.ReadBool(); }

        private static void OnPlayerMoved()
        {
            foreach (Mobile m in Mobiles.Where(m => m.Distance > 20 && !party.Contains(m)))
                RemoveMobile(m);
            foreach (Item i in Ground.Where(i => i.Distance > 20))
                RemoveItem(i);
            ProcessDelta();
        }
    }
}