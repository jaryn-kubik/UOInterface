using System;
using System.Text;

namespace UOInterface
{
    public class Entity
    {
        public static readonly Entity Invalid = new Entity(Serial.Invalid);
        protected Entity(Serial serial) { Serial = serial; }

        public Serial Serial { get; private set; }
        public Graphic Graphic { get; private set; }
        public Hue Hue { get; private set; }
        public string Name { get; private set; }
        public Position Position { get; protected set; }

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
            sb.AppendFormat("Position: {0}", Position);
            ToString(sb);
            return sb.ToString();
        }

        public virtual bool IsValid { get { return Serial.IsValid; } }
        public virtual bool Exists { get { return World.Contains(Serial); } }

        public virtual int DistanceTo(Entity entity) { return Position.DistanceTo(entity.Position); }
        public int Distance { get { return DistanceTo(World.Player); } }

        #region Events
        public event EventHandler AppearanceChanged;
        internal void OnAppearanceChanged(Graphic? graphic = null, Hue? hue = null, string name = null)
        {
            if ((graphic.HasValue && graphic != Graphic) ||
                (hue.HasValue && hue != Hue) ||
                (name != null && name != Name))
            {
                if (graphic.HasValue)
                    Graphic = graphic.Value;
                if (hue.HasValue)
                    Hue = hue.Value;
                if (name != null)
                    Name = name;
                AppearanceChanged.RaiseAsync(this);
            }
        }
        #endregion
    }
}