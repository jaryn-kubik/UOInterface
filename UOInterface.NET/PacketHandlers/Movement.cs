using System.Collections.Generic;
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

            ushort x = p.ReadUShort(2);
            ushort y = p.ReadUShort();
            Direction dir = (Direction)p.ReadByte();
            sbyte z = p.ReadSByte();

            Player.OnMoved(new Position(x, y, z), dir);
        }

        private static void OnMovementAccepted(Packet p)//0x22
        {
            Player.OnAttributesChanged(notoriety: (Notoriety)p.ReadByte(2));
            ProcessMove(movementQueue.Dequeue());
        }

        private static void OnMovementDemand(Packet p)//0x97
        { ProcessMove((Direction)p.ReadByte()); }

        private static void ProcessMove(Direction dir)
        {
            dir &= ~Direction.Running;
            bool moved = dir == Player.Direction;
            if (!moved)
            {
                Player.OnMoved(Player.Position, dir);
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
            Player.OnMoved(new Position(x, y, p.Z), dir);
            OnPlayerMoved();
        }
    }
}