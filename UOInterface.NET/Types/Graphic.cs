using System;
using System.Globalization;

namespace UOInterface
{
    public struct Graphic : IComparable, IComparable<ushort>
    {
        public const ushort Invariant = ushort.MaxValue;

        private readonly ushort value;
        public Graphic(ushort Graphic) { value = Graphic; }

        public bool IsInvariant { get { return value == Invariant; } }

        public static implicit operator Graphic(ushort value) { return new Graphic(value); }
        public static implicit operator ushort(Graphic color) { return color.value; }
        public static bool operator ==(Graphic g1, Graphic g2) { return g1.IsInvariant || g2.IsInvariant || g1.value == g2.value; }
        public static bool operator !=(Graphic g1, Graphic g2) { return !g1.IsInvariant && !g2.IsInvariant && g1.value != g2.value; }

        public int CompareTo(object obj) { return value.CompareTo(obj); }
        public int CompareTo(ushort other) { return value.CompareTo(other); }

        public override string ToString() { return string.Format("0x{0:X4}", value); }
        public override int GetHashCode() { return value.GetHashCode(); }
        public override bool Equals(object obj)
        {
            if (obj is Graphic)
                return this == (Graphic)obj;
            if (obj is ushort)
                return value == (ushort)obj;
            return false;
        }

        public static Graphic Parse(string str) { return ushort.Parse(str, NumberStyles.HexNumber); }
    }
}