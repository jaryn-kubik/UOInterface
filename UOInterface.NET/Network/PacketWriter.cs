using System;

namespace UOInterface.Network
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

        private void WriteSize()
        {
            if (Dynamic)
            {
                data[1] = (byte)(Position);
                data[2] = (byte)(Position >> 8);
            }
        }

        public byte[] Compile()
        {
            Array.Resize(ref data, Position);
            WriteSize();
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

        public void WriteASCII(string value)
        {
            EnsureSize(value.Length + 1);
            foreach (char c in value)
                data[Position++] = (byte)c;
            Position++;
        }

        public void WriteASCII(string value, int length)
        {
            if (value.Length > length)
                throw new ArgumentOutOfRangeException("value");
            EnsureSize(length);
            foreach (char c in value)
                data[Position++] = (byte)c;
            Position += length - value.Length;
        }

        public void WriteUnicode(string value)
        {
            EnsureSize((value.Length + 1) * 2);
            foreach (char c in value)
            {
                data[Position++] = (byte)(c >> 8);
                data[Position++] = (byte)(c);
            }
            Position += 2;
        }

        public void WriteUnicode(string value, int length)
        {
            if (value.Length > length)
                throw new ArgumentOutOfRangeException("value");
            EnsureSize(length * 2);
            foreach (char c in value)
            {
                data[Position++] = (byte)(c >> 8);
                data[Position++] = (byte)(c);
            }
            Position += (length - value.Length) * 2;
        }

        public void SendToClient()
        {
            WriteSize();
            Client.SendToClient(data, Position);
        }

        public void SendToServer()
        {
            WriteSize();
            Client.SendToServer(data, Position);
        }
    }
}