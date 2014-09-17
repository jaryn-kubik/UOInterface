using System;
using System.Globalization;

namespace UOInterface
{
    public struct Serial : IComparable, IComparable<uint>
    {
        public static readonly Serial Invalid = new Serial(0);

        private readonly uint value;
        public Serial(uint serial) { value = serial; }

        public bool IsMobile { get { return value > 0 && value < 0x40000000; } }
        public bool IsItem { get { return value >= 0x40000000 && value < 0x80000000; } }
        public bool IsValid { get { return value > 0 && value < 0x80000000; } }

        public static implicit operator Serial(uint value) { return new Serial(value); }
        public static implicit operator uint(Serial serial) { return serial.value; }
        public static bool operator ==(Serial s1, Serial s2) { return s1.value == s2.value; }
        public static bool operator !=(Serial s1, Serial s2) { return s1.value != s2.value; }
        public static bool operator <(Serial s1, Serial s2) { return s1.value < s2.value; }
        public static bool operator >(Serial s1, Serial s2) { return s1.value > s2.value; }

        public int CompareTo(object obj) { return value.CompareTo(obj); }
        public int CompareTo(uint other) { return value.CompareTo(other); }

        public override string ToString() { return string.Format("0x{0:X8}", value); }
        public override int GetHashCode() { return value.GetHashCode(); }
        public override bool Equals(object obj)
        {
            if (obj is Serial)
                return this == (Serial)obj;
            if (obj is uint)
                return value == (uint)obj;
            return false;
        }

        public static Serial Parse(string str) { return uint.Parse(str, NumberStyles.HexNumber); }
    }
}