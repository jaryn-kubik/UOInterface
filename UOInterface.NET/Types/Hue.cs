using System;
using System.Globalization;

namespace UOInterface
{
    public struct Hue : IComparable, IComparable<ushort>
    {
        public const ushort Invariant = ushort.MaxValue;

        private readonly ushort value;
        public Hue(ushort hue) { value = hue; }

        public bool IsInvariant { get { return value == Invariant; } }

        public static implicit operator Hue(ushort value) { return new Hue(value); }
        public static implicit operator ushort(Hue color) { return color.value; }
        public static bool operator ==(Hue h1, Hue h2) { return h1.IsInvariant || h2.IsInvariant || h1.value == h2.value; }
        public static bool operator !=(Hue h1, Hue h2) { return !h1.IsInvariant && !h2.IsInvariant && h1.value != h2.value; }

        public int CompareTo(object obj) { return value.CompareTo(obj); }
        public int CompareTo(ushort other) { return value.CompareTo(other); }

        public override string ToString() { return string.Format("0x{0:X4}", value); }
        public override int GetHashCode() { return value.GetHashCode(); }
        public override bool Equals(object obj)
        {
            if (obj is Hue)
                return this == (Hue)obj;
            if (obj is ushort)
                return value == (ushort)obj;
            return false;
        }

        public static Hue Parse(string str) { return ushort.Parse(str, NumberStyles.HexNumber); }
    }
}