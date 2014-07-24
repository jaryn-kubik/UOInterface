using System;

namespace UOInterface.Network
{
    public abstract class PacketBase
    {
        protected abstract byte this[int index] { get; set; }
        public abstract int Length { get; }

        public byte Id { get { return this[0]; } }
        public bool Dynamic { get; protected set; }
        public int Position { get; protected set; }

        protected abstract void EnsureSize(int length);
        public abstract byte[] ToArray();

        public void Skip(byte length)
        {
            EnsureSize(length);
            Position += length;
        }

        public void WriteBool(bool value) { WriteByte(value ? (byte)1 : (byte)0); }
        public void WriteSByte(sbyte value) { WriteByte((byte)value); }
        public void WriteByte(byte value)
        {
            EnsureSize(1);
            this[Position++] = value;
        }

        public void WriteUShort(ushort value)
        {
            EnsureSize(2);
            this[Position++] = (byte)(value >> 8);
            this[Position++] = (byte)(value);
        }

        public void WriteUInt(uint value)
        {
            EnsureSize(4);
            this[Position++] = (byte)(value >> 24);
            this[Position++] = (byte)(value >> 16);
            this[Position++] = (byte)(value >> 8);
            this[Position++] = (byte)(value);
        }

        public void WriteASCII(string value)
        {
            EnsureSize(value.Length + 1);
            foreach (char c in value)
                this[Position++] = (byte)c;
            this[Position++] = 0;
        }

        public void WriteASCII(string value, int length)
        {
            EnsureSize(length);
            if (value.Length > length)
                throw new ArgumentOutOfRangeException("value");

            foreach (char c in value)
                this[Position++] = (byte)c;
            if (value.Length < length)
            {
                this[Position++] = 0;
                Position += length - value.Length - 1;
            }
        }

        public void WriteUnicode(string value)
        {
            EnsureSize((value.Length + 1) * 2);
            foreach (char c in value)
            {
                this[Position++] = (byte)(c >> 8);
                this[Position++] = (byte)(c);
            }
            this[Position++] = this[Position++] = 0;
        }

        public void WriteUnicode(string value, int length)
        {
            EnsureSize(length);
            if (value.Length > (length /= 2))
                throw new ArgumentOutOfRangeException("value");

            foreach (char c in value)
            {
                this[Position++] = (byte)(c >> 8);
                this[Position++] = (byte)(c);
            }
            if (value.Length < length)
            {
                this[Position++] = this[Position++] = 0;
                Position += (length - value.Length - 1) * 2;
            }
        }

        public void WriteUnicodeReversed(string value)
        {
            EnsureSize((value.Length + 1) * 2);
            foreach (char c in value)
            {
                this[Position++] = (byte)(c);
                this[Position++] = (byte)(c >> 8);
            }
            this[Position++] = this[Position++] = 0;
        }

        public void WriteUnicodeReversed(string value, int length)
        {
            EnsureSize(length);
            if (value.Length > (length /= 2))
                throw new ArgumentOutOfRangeException("value");

            foreach (char c in value)
            {
                this[Position++] = (byte)(c);
                this[Position++] = (byte)(c >> 8);
            }
            if (value.Length < length)
            {
                this[Position++] = this[Position++] = 0;
                Position += (length - value.Length - 1) * 2;
            }
        }
    }
}