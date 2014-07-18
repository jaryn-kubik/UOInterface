using System;
using System.Text;

namespace UOInterface
{
    public class Entity
    {
        public static readonly Entity Invalid = new Entity(Serial.Invalid);
        protected Entity(Serial serial) { Serial = serial; }

        protected readonly object syncRoot = new object();
        internal object SyncRoot { get { return syncRoot; } }

        [Flags]
        protected enum EntityDelta { Appearance, Position, Attributes }
        protected EntityDelta entityDelta;

        private Serial serial;
        private Graphic graphic;
        private Hue hue;
        private string name;
        private Position position;
        private Direction direction;
        private UOFlags flags;

        public Serial Serial
        {
            get { lock (syncRoot) return serial; }
            internal set { serial = value; }
        }

        public Graphic Graphic
        {
            get { lock (syncRoot) return graphic; }
            internal set
            {
                if (graphic != value)
                {
                    graphic = value;
                    entityDelta |= EntityDelta.Appearance;
                }
            }
        }

        public Hue Hue
        {
            get { lock (syncRoot) return hue; }
            internal set
            {
                if (hue != value)
                {
                    hue = value;
                    entityDelta |= EntityDelta.Appearance;
                }
            }
        }

        public string Name
        {
            get { lock (syncRoot) return name; }
            internal set
            {
                if (name != value)
                {
                    name = value;
                    entityDelta |= EntityDelta.Appearance;
                }
            }
        }

        public Position Position
        {
            get { lock (syncRoot) return position; }
            internal set
            {
                if (position != value)
                {
                    position = value;
                    entityDelta |= EntityDelta.Position;
                }
            }
        }

        public Direction Direction
        {
            get { lock (syncRoot) return direction; }
            internal set
            {
                if (direction != value)
                {
                    direction = value;
                    entityDelta |= EntityDelta.Position;
                }
            }
        }

        public UOFlags Flags
        {
            get { lock (syncRoot) return flags; }
            internal set
            {
                if (flags != value)
                {
                    flags = value;
                    entityDelta |= EntityDelta.Attributes;
                }
            }
        }

        public event EventHandler AppearanceChanged;
        public event EventHandler PositionChanged;
        public event EventHandler AttributesChanged;
        internal virtual void ProcessDelta()
        {
            if (entityDelta.HasFlag(EntityDelta.Appearance))
                AppearanceChanged.RaiseAsync(this);

            if (entityDelta.HasFlag(EntityDelta.Position))
                PositionChanged.RaiseAsync(this);

            if (entityDelta.HasFlag(EntityDelta.Attributes))
                AttributesChanged.RaiseAsync(this);
        }

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
            sb.AppendFormat("Direction: {0}\n", Direction);
            sb.AppendFormat("Flags: {0}", Flags);
            ToString(sb);
            return sb.ToString();
        }

        public virtual bool IsValid { get { return Serial.IsValid; } }
        public virtual bool Exists { get { return World.Contains(Serial); } }

        public virtual int DistanceTo(Entity entity) { return Position.DistanceTo(entity.Position); }
        public int Distance { get { return DistanceTo(World.Player); } }
    }
}