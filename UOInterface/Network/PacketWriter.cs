using System;

namespace UOInterface.Network
{
    public class PacketWriter : PacketBase
    {
        private byte[] data;
        public PacketWriter(byte id)
        {
            short len = Client.GetPacketLength(id);
            Dynamic = len < 0;
            data = Dynamic ? new byte[64] : new byte[len];
            data[0] = id;
            Position = Dynamic ? 3 : 1;
        }

        public override int Length { get { return data.Length; } }
        protected override byte this[int index]
        {
            get { return data[index]; }
            set { data[index] = value; }
        }

        protected override void EnsureSize(int length)
        {
            if (length < 0)
                throw new ArgumentOutOfRangeException("length");

            if (Dynamic)
                while (Position + length > Length)
                    Array.Resize(ref data, Length * 2);
            else if (Position + length > Length)
                throw new ArgumentOutOfRangeException("length");
        }

        private void WriteSize()
        {
            if (Dynamic)
            {
                this[1] = (byte)(Position);
                this[2] = (byte)(Position >> 8);
            }
        }

        public override byte[] ToArray()
        {
            Array.Resize(ref data, Position);
            WriteSize();
            return data;
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