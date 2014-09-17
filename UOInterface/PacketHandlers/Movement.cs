using System.Collections.Generic;
using System.Linq;
using UOInterface.Network;

namespace UOInterface
{
    public static partial class World
    {
        private static readonly Queue<Direction> movementQueue = new Queue<Direction>();

        private static void OnMovementRequest(Packet p)//0x02
        { movementQueue.Enqueue((Direction)p.ReadByte()); }

        private static void OnResyncRequest(Packet p)//0x22
        { movementQueue.Clear(); }

        private static void OnMovementRejected(Packet p)//0x21
        {
            movementQueue.Clear();
            p.Skip(1);
            ushort x = p.ReadUShort();
            ushort y = p.ReadUShort();
            Player.Direction = (Direction)p.ReadByte();
            Player.Position = new Position(x, y, p.ReadSByte());
            Player.ProcessDelta();
        }

        private static void OnMovementAccepted(Packet p)//0x22
        {
            p.Skip(1);
            Player.Notoriety = (Notoriety)p.ReadByte();
            if (movementQueue.Count > 0)
                ProcessMove(movementQueue.Dequeue());
            Player.ProcessDelta();
        }

        private static void OnMovementDemand(Packet p) //0x97
        {
            ProcessMove((Direction)p.ReadByte());
            Player.ProcessDelta();
        }

        private static void ProcessMove(Direction dir)
        {
            dir &= ~Direction.Running;
            if (dir != Player.Direction)
            {
                Player.Direction = dir;
                return;
            }
            Position p = Player.Position;
            ushort x = p.X;
            ushort y = p.Y;
            switch (dir)
            {
                case Direction.North:
                    y--;
                    break;
                case Direction.Right:
                    y--;
                    x++;
                    break;
                case Direction.East:
                    x++;
                    break;
                case Direction.Down:
                    y++;
                    x++;
                    break;
                case Direction.South:
                    y++;
                    break;
                case Direction.Left:
                    y++;
                    x--;
                    break;
                case Direction.West:
                    x--;
                    break;
                case Direction.Up:
                    y--;
                    x--;
                    break;
            }
            Player.Position = new Position(x, y, p.Z);
            OnPlayerMoved();
        }

        private static void OnPlayerMoved()
        {
            foreach (Mobile m in Mobiles.Where(m => m.Distance > updateRange && !party.Contains(m)))
                RemoveMobile(m);
            Mobiles.ProcessDelta();

            foreach (Item i in Ground.Where(i => i.Distance > updateRange))
                RemoveItem(i);
            Items.ProcessDelta();
        }
    }
}