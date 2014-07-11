using System;
using System.Text;

namespace UOInterface
{
    public class PacketWriter
    {
        private byte[] data;
        public int ID { get { return data[0]; } }
        public int Position { get; private set; }
        public bool Dynamic { get; private set; }

        public PacketWriter(byte id)
        {
            short len = Client.GetPacketLength(id);
            Dynamic = len < 0;
            data = Dynamic ? new byte[64] : new byte[len];
            data[0] = id;
            Position = Dynamic ? 3 : 1;
        }

        private void EnsureSize(int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException("length");

            if (Dynamic)
                while (Position + length > data.Length)
                    Array.Resize(ref data, data.Length * 2);
            else if (Position + length > data.Length)
                throw new ArgumentOutOfRangeException("length");
        }

        public byte[] Compile()
        {
            Array.Resize(ref data, Position);
            if (Dynamic)
            {
                data[1] = (byte)(data.Length >> 8);
                data[2] = (byte)(data.Length);
            }
            return data;
        }

        public void Skip(int length)
        {
            EnsureSize(length);
            Position += length;
        }

        public void WriteBool(bool value)
        {
            EnsureSize(1);
            data[Position++] = value ? (byte)1 : (byte)0;
        }

        public void WriteByte(byte value)
        {
            EnsureSize(1);
            data[Position++] = value;
        }

        public void WriteUShort(ushort value)
        {
            EnsureSize(2);
            data[Position++] = (byte)(value >> 8);
            data[Position++] = (byte)(value);
        }

        public void WriteUInt(uint value)
        {
            EnsureSize(4);
            data[Position++] = (byte)(value >> 24);
            data[Position++] = (byte)(value >> 16);
            data[Position++] = (byte)(value >> 8);
            data[Position++] = (byte)(value);
        }

        public void WriteStringAscii(string value)
        {
            EnsureSize(value.Length + 1);
            Encoding.ASCII.GetBytes(value, 0, value.Length, data, Position);
            Position += value.Length + 1;
        }

        public void WriteStringAscii(string value, int lenght)
        {
            if (value.Length > lenght)
                throw new ArgumentOutOfRangeException("lenght");
            EnsureSize(lenght);
            Encoding.ASCII.GetBytes(value, 0, value.Length, data, Position);
            Position += lenght;
        }

        /*public void WriteStringUnicode(string value)
        {
            EnsureSize(value.Length * 2 + 2);
            Encoding.BigEndianUnicode.GetBytes(value, 0, value.Length, data, Position);
            Position += value.Length * 2 + 2;
        }

        public void WriteStringUnicode(string value, int lenght)
        {
            if (value.Length > lenght)
                throw new ArgumentOutOfRangeException("lenght");
            EnsureSize(lenght);
            Encoding.BigEndianUnicode.GetBytes(value, 0, value.Length, data, Position);
            Position += lenght;
        }*/
    }
}