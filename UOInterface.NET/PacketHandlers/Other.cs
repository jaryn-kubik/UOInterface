using UOInterface.Network;

namespace UOInterface
{
    public static partial class World
    {
        private static void OnInfravision(Packet p)//0x4E
        { p.WriteByte(5, 0x1F); }

        private static void OnGlobalLight(Packet p)//0x4F
        { p.WriteByte(0x1F); }

        private static void OnRemoveObject(Packet p)//0x1D
        { Remove(p.ReadUInt()); }
    }
}