using System;
using System.Runtime.InteropServices;
using System.Text;

namespace UOInterface.Network
{
    public unsafe class Packet
    {
        private readonly byte* data;

        public byte ID { get { return data[0]; } }
        public bool Dynamic { get; private set; }
        public int Length { get; private set; }
        public int Position { get; private set; }
        public bool Changed { get; private set; }
        public bool Filter { get; set; }

        internal Packet(byte* data, int len)
        {
            this.data = data;
            Length = len;
            Dynamic = Client.GetPacketLength(ID) < 0;
        }

        public void MoveToData() { EnsureSize(Dynamic ? 3 : 1, 0); }
        public void Skip(byte length) { EnsureSize(Position, length); }
        public void Seek(int index) { EnsureSize(index, 0); }

        public byte this[int index]
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

        public byte[] ToArray()
        {
            byte[] bytes = new byte[Length];
            Marshal.Copy(new IntPtr(data), bytes, 0, Length);
            return bytes;
        }

        private void EnsureSize(int index, int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException("length");
            if (index < 0 || index + length > Length)
                throw new ArgumentOutOfRangeException("index");
            Position = index + length;
        }

        #region Read
        public bool ReadBool() { return ReadBool(Position); }
        public bool ReadBool(int index) { return ReadByte(index) != 0; }

        public sbyte ReadSByte() { return ReadSByte(Position); }
        public sbyte ReadSByte(int index) { return (sbyte)ReadByte(index); }

        public byte ReadByte() { return ReadByte(Position); }
        public byte ReadByte(int index)
        {
            EnsureSize(index, 1);
            return data[index];
        }

        public ushort ReadUShort() { return ReadUShort(Position); }
        public ushort ReadUShort(int index)
        {
            EnsureSize(index, 2);
            return (ushort)((data[index++] << 8) |
                            data[index]);
        }

        public uint ReadUInt() { return ReadUInt(Position); }
        public uint ReadUInt(int index)
        {
            EnsureSize(index, 4);
            return (uint)((data[index++] << 24) |
                            data[index++] << 16 |
                            data[index++] << 8 |
                            data[index]);
        }

        public string ReadStringAscii(int length) { return ReadStringAscii(Position, length); }
        public string ReadStringAscii(int index, int length)
        {
            EnsureSize(index, length);
            StringBuilder sb = new StringBuilder(length);
            byte c;
            while (index < Position && (c = data[index++]) != '\0')
                sb.Append((char)c);
            return sb.ToString();
        }

        public string ReadStringAsciiNull() { return ReadStringAsciiNull(Position); }
        public string ReadStringAsciiNull(int index)
        {
            EnsureSize(index, 1);
            StringBuilder sb = new StringBuilder();
            byte c;
            while (index < Length - 1 && (c = data[index++]) != '\0')
                sb.Append((char)c);
            Position = index;
            return sb.ToString();
        }

        public string ReadStringUnicode(int length) { return ReadStringUnicode(Position, length); }
        public string ReadStringUnicode(int index, int length)
        {
            EnsureSize(index, length * 2);
            StringBuilder sb = new StringBuilder(length);
            int c;
            while (index < Position && (c = data[index++] << 8 | data[index++]) != '\0')
                sb.Append((char)c);
            return sb.ToString();
        }

        public string ReadStringUnicodeNull() { return ReadStringUnicodeNull(Position); }
        public string ReadStringUnicodeNull(int index)
        {
            EnsureSize(index, 2);
            StringBuilder sb = new StringBuilder();
            int c;
            while (index < Length - 2 && (c = data[index++] << 8 | data[index++]) != '\0')
                sb.Append((char)c);
            Position = index;
            return sb.ToString();
        }
        #endregion

        #region Write
        public void WriteBool(bool value) { WriteBool(Position, value); }
        public void WriteBool(int index, bool value)
        {
            EnsureSize(index, 1);
            data[index] = value ? (byte)1 : (byte)0;
            Changed = true;
        }

        public void WriteByte(byte value) { WriteByte(Position, value); }
        public void WriteByte(int index, byte value)
        {
            EnsureSize(index, 1);
            data[index] = value;
            Changed = true;
        }

        public void WriteUShort(ushort value) { WriteUShort(Position, value); }
        public void WriteUShort(int index, ushort value)
        {
            EnsureSize(index, 2);
            data[index] = (byte)(value >> 8);
            data[index + 1] = (byte)(value);
            Changed = true;
        }

        public void WriteUInt(uint value) { WriteUInt(Position, value); }
        public void WriteUInt(int index, uint value)
        {
            EnsureSize(index, 4);
            data[index] = (byte)(value >> 24);
            data[index + 1] = (byte)(value >> 16);
            data[index + 2] = (byte)(value >> 8);
            data[index + 3] = (byte)(value);
            Changed = true;
        }

        public void WriteStringAscii(string value) { WriteStringAscii(Position, value); }
        public void WriteStringAscii(int index, string value)
        {
            EnsureSize(index, value.Length + 1);
            foreach (char c in value)
                data[index++] = (byte)c;
            data[index] = 0;
            Changed = true;
        }

        public void WriteStringAscii(string value, int length) { WriteStringAscii(Position, value, length); }
        public void WriteStringAscii(int index, string value, int length)
        {
            if (value.Length > length)
                throw new ArgumentOutOfRangeException("value");
            EnsureSize(index, length);
            foreach (char c in value)
                data[index++] = (byte)c;
            while (index < Position)
                data[index++] = 0;
            Changed = true;
        }

        public void WriteStringUnicode(string value) { WriteStringUnicode(Position, value); }
        public void WriteStringUnicode(int index, string value)
        {
            EnsureSize(index, (value.Length + 1) * 2);
            foreach (char c in value)
            {
                data[index++] = (byte)(c >> 8);
                data[index++] = (byte)(c);
            }
            data[index++] = data[index] = 0;
            Changed = true;
        }

        public void WriteStringUnicode(string value, int length) { WriteStringUnicode(Position, value, length); }
        public void WriteStringUnicode(int index, string value, int length)
        {
            if (value.Length > length)
                throw new ArgumentOutOfRangeException("value");
            EnsureSize(index, length * 2);
            foreach (char c in value)
            {
                data[index++] = (byte)(c >> 8);
                data[index++] = (byte)(c);
            }
            while (index < Position)
                data[index++] = 0;
            Changed = true;
        }
        #endregion
    }
}