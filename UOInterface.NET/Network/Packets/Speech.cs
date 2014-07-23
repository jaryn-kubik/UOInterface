namespace UOInterface.Network
{
    public class MessageUnicode : PacketWriter
    {
        public MessageUnicode(Serial serial, Graphic graphic, MessageType type, Hue hue,
            MessageFont font, string lang, string name, string text)
            : base(0xAE)
        {
            WriteUInt(serial);
            WriteUShort(graphic);
            WriteByte((byte)type);
            WriteUShort(hue);
            WriteUShort((ushort)font);
            WriteASCII(lang, 4);
            WriteASCII(name, 30);
            WriteUnicode(text);
        }
    }

    public class MessageUnicodeRequest : PacketWriter
    {
        public MessageUnicodeRequest(MessageType type, Hue hue, MessageFont font, string lang, string text)
            : base(0xAD)
        {
            WriteByte((byte)type);
            WriteUShort(hue);
            WriteUShort((ushort)font);
            WriteASCII(lang, 4);
            WriteUnicode(text);
        }
    }
}