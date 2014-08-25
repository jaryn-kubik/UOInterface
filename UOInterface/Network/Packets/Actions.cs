namespace UOInterface.Network
{
    public class LiftReject : PacketWriter
    {
        public enum Reason { Holding = 4 }
        public LiftReject(Reason reason) : base(0x27) { WriteByte((byte)reason); }
    }

    public class LiftRequest : PacketWriter
    {
        public LiftRequest(Serial serial, ushort amount)
            : base(0x07)
        {
            WriteUInt(serial);
            WriteUShort(amount);
        }
    }
}