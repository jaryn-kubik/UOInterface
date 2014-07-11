using System;
using System.Runtime.InteropServices;

namespace UOInterface
{
    public unsafe class Packet
    {
        private readonly byte* data;

        public byte ID { get { return data[0]; } }
        public bool Dynamic { get; private set; }
        public int Length { get; private set; }
        public int Position { get; private set; }

        internal Packet(byte* data, int len)
        {
            this.data = data;
            Length = len;
            Dynamic = Client.GetPacketLength(ID) < 0;
            Position = Dynamic ? 3 : 1;
        }

        public void MoveToData() { EnsureSize(Dynamic ? 3 : 1, 0); }
        public void MoveToEnd() { EnsureSize(Length, 0); }
        public void Skip(byte lenght) { EnsureSize(Position, lenght); }
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
            if (index < 0)
                throw new ArgumentOutOfRangeException("index");
            Position = index + length;
            if (Position > Length)
                throw new ArgumentOutOfRangeException("index");
        }

        #region Read
        public bool ReadBool() { return ReadBool(Position); }
        public bool ReadBool(int index)
        {
            EnsureSize(index, 1);
            return data[index] != 0;
        }

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
            return (ushort)((data[index] << 8) |
                            data[index + 1]);
        }

        public uint ReadUInt() { return ReadUInt(Position); }
        public uint ReadUInt(int index)
        {
            EnsureSize(index, 4);
            return (uint)((data[index] << 24) |
                            data[index + 1] << 16 |
                            data[index + 2] << 8 |
                            data[index + 3]);
        }
        #endregion

        #region Write
        public void WriteBool(bool value) { WriteBool(Position, value); }
        public void WriteBool(int index, bool value)
        {
            EnsureSize(index, 1);
            data[index] = value ? (byte)1 : (byte)0;
        }

        public void WriteByte(byte value) { WriteByte(Position, value); }
        public void WriteByte(int index, byte value)
        {
            EnsureSize(index, 1);
            data[index] = value;
        }

        public void WriteUShort(ushort value) { WriteUShort(Position, value); }
        public void WriteUShort(int index, ushort value)
        {
            EnsureSize(index, 2);
            data[index] = (byte)(value >> 8);
            data[index + 1] = (byte)(value);
        }

        public void WriteUInt(uint value) { WriteUInt(Position, value); }
        public void WriteUInt(int index, uint value)
        {
            EnsureSize(index, 4);
            data[index] = (byte)(value >> 24);
            data[index + 1] = (byte)(value >> 16);
            data[index + 2] = (byte)(value >> 8);
            data[index + 3] = (byte)(value);
        }
        #endregion
    }
}