using System;
using System.Runtime.InteropServices;
using System.Text;

namespace UOInterface.Network
{
    public unsafe sealed class Packet : PacketBase
    {
        private readonly byte* data;
        private readonly int len;
        internal Packet(byte* data, int len)
        {
            this.data = data;
            this.len = len;
            Dynamic = Client.GetPacketLength(Id) < 0;
        }

        public override int Length { get { return len; } }
        public bool Changed { get; private set; }
        public bool Filter { get; set; }

        protected override byte this[int index]
        {
            get
            {
                if (index < 0 || index >= Length)
                    throw new ArgumentOutOfRangeException("index");
                return data[index];
            }
            set
            {
                if (index < 0 || index >= Length)
                    throw new ArgumentOutOfRangeException("index");
                data[index] = value;
                Changed = true;
            }
        }

        protected override void EnsureSize(int length)
        {
            if (length < 0 || Position + length > Length)
                throw new ArgumentOutOfRangeException("length");
        }

        public override byte[] ToArray()
        {
            byte[] bytes = new byte[Length];
            Marshal.Copy(new IntPtr(data), bytes, 0, Length);
            return bytes;
        }

        public void MoveToData() { Seek(Dynamic ? 3 : 1); }
        public void Seek(int index)
        {
            Position = index;
            EnsureSize(0);
        }

        public bool ReadBool() { return ReadByte() != 0; }
        public sbyte ReadSByte() { return (sbyte)ReadByte(); }
        public byte ReadByte()
        {
            EnsureSize(1);
            return this[Position++];
        }

        public ushort ReadUShort()
        {
            EnsureSize(2);
            return (ushort)((this[Position++] << 8) |
                             this[Position++]);
        }

        public uint ReadUInt()
        {
            EnsureSize(4);
            return (uint)((this[Position++] << 24) |
                           this[Position++] << 16 |
                           this[Position++] << 8 |
                           this[Position++]);
        }

        public string ReadASCII()
        {
            EnsureSize(1);
            StringBuilder sb = new StringBuilder();
            byte c;
            while (Position < Length && (c = this[Position++]) != '\0')
                sb.Append((char)c);
            return sb.ToString();
        }

        public string ReadASCII(int length)
        {
            EnsureSize(length);
            byte[] buffer = new byte[length + 1];
            for (int i = 0; i < length; i++)
                buffer[i] = this[Position++];
            fixed (byte* str = buffer)
                return new string((sbyte*)str);
        }

        public string ReadUnicode()
        {
            EnsureSize(2);
            StringBuilder sb = new StringBuilder();
            int c;
            while (Position < Length - 1 && (c = this[Position++] << 8 | this[Position++]) != '\0')
                sb.Append((char)c);
            return sb.ToString();
        }

        public string ReadUnicode(int length)
        {
            EnsureSize(length);
            length /= 2;
            char[] buffer = new char[length + 1];
            for (int i = 0; i < length; i++)
                buffer[i] = (char)(this[Position++] << 8 | this[Position++]);
            fixed (char* str = buffer)
                return new string(str);
        }

        public string ReadUnicodeReversed()
        {
            EnsureSize(2);
            StringBuilder sb = new StringBuilder();
            int c;
            while (Position < Length - 1 && (c = this[Position++] | this[Position++] << 8) != '\0')
                sb.Append((char)c);
            return sb.ToString();
        }

        public string ReadUnicodeReversed(int length)
        {
            EnsureSize(length);
            length /= 2;
            char[] buffer = new char[length + 1];
            for (int i = 0; i < length; i++)
                buffer[i] = (char)(this[Position++] | this[Position++] << 8);
            fixed (char* str = buffer)
                return new string(str);
        }
    }
}