using System;

namespace UOInterface
{
    public struct Position
    {
        public ushort X { get; set; }
        public ushort Y { get; set; }
        public sbyte Z { get; set; }

        public Position(ushort x, ushort y, sbyte z = 0)
            : this()
        {
            X = x;
            Y = y;
            Z = z;
        }

        public static bool operator ==(Position p1, Position p2) { return p1.X == p2.X && p1.Y == p2.Y && p1.Z == p2.Z; }
        public static bool operator !=(Position p1, Position p2) { return p1.X != p2.X || p1.Y != p2.Y || p1.Z != p2.Z; }

        public int DistanceTo(Position position) { return Math.Max(Math.Abs(position.X - X), Math.Abs(position.Y - Y)); }
        public double DistanceToSqrt(Position position)
        {
            int a = position.X - X;
            int b = position.Y - Y;
            return Math.Sqrt(a * a + b * b);
        }

        public override int GetHashCode() { return X ^ Y ^ Z; }
        public override bool Equals(object obj) { return obj is Position && this == (Position)obj; }
        public override string ToString() { return string.Format("{0}.{1}.{2}", X, Y, Z); }
        public static Position Parse(string str)
        {
            string[] args = str.Split('.');
            return new Position(ushort.Parse(args[0]), ushort.Parse(args[1]), sbyte.Parse(args[2]));
        }
    }
}