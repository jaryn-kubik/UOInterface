using System.Text;

namespace UOInterface
{
    public class Entity
    {
        public static readonly Entity Invalid = new Entity(Serial.Invalid);
        protected Entity(Serial serial) { Serial = serial; }

        public Serial Serial { get; private set; }
        public Graphic Graphic { get; internal set; }
        public Hue Hue { get; internal set; }
        public string Name { get; internal set; }
        public UOFlags Flags { get; internal set; }
        public Position Position { get; internal set; }

        public static implicit operator Serial(Entity entity) { return entity.Serial; }
        public static implicit operator uint(Entity entity) { return entity.Serial; }
        public override int GetHashCode() { return Serial.GetHashCode(); }

        protected virtual void ToString(StringBuilder sb) { }
        public sealed override string ToString()
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendFormat("Serial: {0}\n", Serial);
            sb.AppendFormat("Graphic: {0}\n", Graphic);
            sb.AppendFormat("Hue: {0}\n", Hue);
            if (!string.IsNullOrEmpty(Name))
                sb.AppendFormat("Name: '{0}'\n", Name);
            sb.AppendFormat("Position: {0}\n", Position);
            sb.AppendFormat("Flags: {0}\n\n", Flags);
            ToString(sb);
            return sb.ToString().Trim();
        }

        public virtual bool IsValid { get { return Serial.IsValid; } }
        public virtual bool Exists { get { return World.Contains(Serial); } }
    }
}