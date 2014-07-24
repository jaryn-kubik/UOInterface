using UOInterface.Network;

namespace UOInterface
{
    public static partial class World
    {
        private static void OnASCIIMessage(Packet p)//0x1C
        {
            Entity entity = GetMobile(p.ReadUInt());
            ushort graphic = p.ReadUShort();
            MessageType type = (MessageType)p.ReadByte();
            Hue hue = p.ReadUShort();
            MessageFont font = (MessageFont)p.ReadUShort();
            string name = p.ReadASCII(30);
            string text = p.ReadASCII();

            if (entity != null)
            {
                entity.Graphic = graphic;
                entity.Name = name;
                toProcess.Enqueue(entity);
                ProcessDelta();
            }
            Chat.OnMessage(entity, new UOMessageEventArgs(text, hue, type, font));
        }

        private static void OnUnicodeMessage(Packet p)//0xAE
        {
            Entity entity = GetMobile(p.ReadUInt());
            ushort graphic = p.ReadUShort();
            MessageType type = (MessageType)p.ReadByte();
            Hue hue = p.ReadUShort();
            MessageFont font = (MessageFont)p.ReadUShort();
            string lang = p.ReadASCII(4);
            string name = p.ReadASCII(30);
            string text = p.ReadUnicode();

            if (entity != null)
            {
                entity.Graphic = graphic;
                entity.Name = name;
                toProcess.Enqueue(entity);
                ProcessDelta();
            }
            Chat.OnMessage(entity, new UOMessageEventArgs(text, hue, type, font, lang));
        }

        private static void OnLocalizedMessage(Packet p)//0xC1
        {
            Entity entity = GetMobile(p.ReadUInt());
            ushort graphic = p.ReadUShort();
            MessageType type = (MessageType)p.ReadByte();
            Hue hue = p.ReadUShort();
            MessageFont font = (MessageFont)p.ReadUShort();
            uint cliloc = p.ReadUInt();
            string name = p.ReadASCII(30);
            string text = p.ReadUnicodeReversed();

            if (entity != null)
            {
                entity.Graphic = graphic;
                entity.Name = name;
                toProcess.Enqueue(entity);
                ProcessDelta();
            }
            Chat.OnLocalizedMessage(entity, new UOMessageEventArgs(text, hue, type, font, cliloc));
        }

        private static void OnLocalizedMessageAffix(Packet p)//0xCC
        {
            Entity entity = GetMobile(p.ReadUInt());
            ushort graphic = p.ReadUShort();
            MessageType type = (MessageType)p.ReadByte();
            Hue hue = p.ReadUShort();
            MessageFont font = (MessageFont)p.ReadUShort();
            uint cliloc = p.ReadUInt();
            AffixType affixType = (AffixType)p.ReadByte();
            string name = p.ReadASCII(30);
            string affix = p.ReadASCII();
            string text = p.ReadUnicode();

            if (entity != null)
            {
                entity.Graphic = graphic;
                entity.Name = name;
                toProcess.Enqueue(entity);
                ProcessDelta();
            }
            Chat.OnLocalizedMessage(entity, new UOMessageEventArgs(text, hue, type, font, cliloc, affixType, affix));
        }
    }
}