using UOInterface.Network;

namespace UOInterface
{
    public static partial class World
    {
        private static void OnBigFuckingPacket(Packet p)//0xBF
        {
            switch (p.ReadUShort())
            {
                case 6://party
                    switch (p.ReadByte())
                    {
                        case 1:
                            lock (party)
                            {
                                party.Clear();
                                byte count = p.ReadByte();
                                for (int i = 0; i < count; i++)
                                    party.Add(p.ReadUInt());
                            }
                            break;
                        case 2:
                            lock (party)
                            {
                                party.Clear();
                                byte count = p.ReadByte();
                                p.Skip(4);
                                for (int i = 0; i < count; i++)
                                    party.Add(p.ReadUInt());
                            }
                            break;
                    }
                    break;

                case 8://map change
                    Map = (Map)p.ReadByte();
                    MapChanged.RaiseAsync();
                    break;
            }
        }
    }
}