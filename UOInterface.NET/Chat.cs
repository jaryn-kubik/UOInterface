using System;
using System.Globalization;
using System.Threading.Tasks;
using UOInterface.Network;

namespace UOInterface
{
    public static class Chat
    {
        private const ushort defaultHue = 0x0017;
        private static readonly string language = CultureInfo.CurrentCulture.ThreeLetterWindowsLanguageName;
        private static readonly Mobile system = new Mobile(Serial.Invalid) { Graphic = Graphic.Invariant, Name = "System" };

        public static void Print(string message, ushort hue = defaultHue,
            MessageType type = MessageType.Regular, MessageFont font = MessageFont.Normal)
        { Print(system, message, hue, type, font); }

        public static void Print(this Entity entity, string message, ushort hue = defaultHue,
            MessageType type = MessageType.Regular, MessageFont font = MessageFont.Normal)
        {
            new MessageUnicode(entity.Serial, entity.Graphic, type, hue, font, language, entity.Name ?? string.Empty, message)
                .SendToClient();
        }

        public static void Say(string message, ushort hue = defaultHue,
            MessageType type = MessageType.Regular, MessageFont font = MessageFont.Normal)
        {
            new MessageUnicodeRequest(type, hue, font, language, message)
                .SendToServer();
        }

        public static event EventHandler<UOMessageEventArgs> Message;
        public static event EventHandler<UOMessageEventArgs> LocalizedMessage;
        internal static void OnMessage(Entity entity, UOMessageEventArgs args)
        { Task.Run(() => Message.Raise(args, entity ?? system)); }

        internal static void OnLocalizedMessage(Entity entity, UOMessageEventArgs args)
        { Task.Run(() => LocalizedMessage.Raise(args, entity ?? system)); }
    }

    public class UOMessageEventArgs : EventArgs
    {
        public UOMessageEventArgs(string text, Hue hue, MessageType type, MessageFont font, string lang = null)
        {
            Text = text;
            Hue = hue;
            Type = type;
            Font = font;
            Language = lang;
            AffixType = AffixType.None;
        }

        public UOMessageEventArgs(string text, Hue hue, MessageType type, MessageFont font,
            uint cliloc, AffixType affixType = AffixType.None, string affix = null)
        {
            Text = text;
            Hue = hue;
            Type = type;
            Font = font;
            Cliloc = cliloc;
            AffixType = affixType;
            Affix = affix;
        }

        public string Text { get; private set; }
        public Hue Hue { get; private set; }
        public MessageType Type { get; private set; }
        public MessageFont Font { get; private set; }
        public string Language { get; private set; }
        public uint Cliloc { get; private set; }
        public AffixType AffixType { get; private set; }
        public string Affix { get; private set; }
    }
}
