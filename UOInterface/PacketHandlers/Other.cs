using UOInterface.Network;

namespace UOInterface
{
    public static partial class World
    {
        private static void OnRemoveObject(Packet p) //0x1D
        {
            Serial serial = p.ReadUInt();
            if (serial.IsItem)
            {
                if (RemoveItem(serial))
                    Items.ProcessDelta();
            }
            else if (serial.IsMobile && RemoveMobile(serial))
            {
                Items.ProcessDelta();
                Mobiles.ProcessDelta();
            }
        }

        private static void OnBigFuckingPacket(Packet p)//0xBF
        {
            switch (p.ReadUShort())
            {
                case 6://party
                    switch (p.ReadByte())
                    {
                        case 1:
                            {
                                byte count = p.ReadByte();
                                for (int i = 0; i < 10; i++)
                                    party[i] = i < count ? p.ReadUInt() : 0;
                            }
                            break;
                        case 2:
                            {
                                byte count = p.ReadByte();
                                p.Skip(4);
                                for (int i = 0; i < 10; i++)
                                    party[i] = i < count ? p.ReadUInt() : 0;
                            }
                            break;
                    }
                    break;

                case 8://map change
                    Map = (Map)p.ReadByte();
                    MapChanged.Raise();
                    break;
            }
        }

        private static void OnChangeUpdateRange(Packet p) //0xC8
        { updateRange = p.ReadByte(); }
    }
}