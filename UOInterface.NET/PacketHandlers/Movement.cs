using System.Collections.Generic;
using UOInterface.Network;

namespace UOInterface
{
    public static partial class World
    {
        private static readonly Queue<Direction> movementQueue = new Queue<Direction>();

        private static void OnMovementRequest(Packet p)//0x02
        { movementQueue.Enqueue((Direction)p.ReadByte() & ~Direction.Running); }

        private static void OnResyncRequest(Packet p)//0x22
        { movementQueue.Clear(); }

        private static void OnMovementRejected(Packet p)//0x21
        {
            movementQueue.Clear();

            ushort x = p.ReadUShort(2);
            ushort y = p.ReadUShort();
            Player.Direction = (Direction)p.ReadByte() & ~Direction.Running;
            sbyte z = p.ReadSByte();

            Player.OnPositionChanged(new Position(x, y, z));
        }

        private static void OnMovementAccepted(Packet p)//0x22
        {
            Player.Notoriety = (Notoriety)p.ReadByte(2);
            ProcessMove(movementQueue.Dequeue());
        }

        private static void OnMovementDemand(Packet p)//0x97
        { ProcessMove((Direction)p.ReadByte()); }

        private static void ProcessMove(Direction dir)
        {
            dir &= ~Direction.Running;
            bool moved = dir == Player.Direction;
            Player.Direction = dir;
            if (!moved)
                return;
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
            Player.OnPositionChanged(new Position(x, y, p.Z));
            OnPlayerMoved();
        }
    }
}