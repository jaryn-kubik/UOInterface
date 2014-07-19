using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace UOInterface
{
    public class Entity
    {
        public static readonly Entity Invalid = new Entity(Serial.Invalid);
        protected Entity(Serial serial) { Serial = serial; }

        protected readonly object syncRoot = new object();
        internal object SyncRoot { get { return syncRoot; } }

        [Flags]
        protected enum Delta
        {
            None = 0,
            Appearance = (1 << 0),
            Position = (1 << 1),
            Attributes = (1 << 2),
            Ownership = (1 << 3),
            Hits = (1 << 4),
            Mana = (1 << 5),
            Stamina = (1 << 6),
            Stats = (1 << 7)
        }
        protected Delta delta;

        private Serial serial;
        private Graphic graphic;
        private Hue hue;
        private string name;
        private Position position;
        private Direction direction;
        private UOFlags flags;
        private readonly ConcurrentDictionary<Serial, Item> items = new ConcurrentDictionary<Serial, Item>(1, 32);

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
                    delta |= Delta.Appearance;
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
                    delta |= Delta.Appearance;
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
                    delta |= Delta.Appearance;
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
                    delta |= Delta.Position;
                }
            }
        }

        public Direction Direction
        {
            get { lock (syncRoot) return direction; }
            internal set
            {
                direction &= Direction.Running;
                if (direction != value)
                {
                    direction = value;
                    delta |= Delta.Position;
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
                    delta |= Delta.Attributes;
                }
            }
        }

        private readonly List<Item> added = new List<Item>(), removed = new List<Item>();
        public IEnumerable<Item> Items { get { return items.Select(item => item.Value); } }
        internal void AddItem(Item item)
        {
            if (items.TryAdd(item.Serial, item))
                added.Add(item);
        }

        internal void RemoveItem(Item item)
        {
            if (items.TryRemove(item.serial, out item))
                removed.Add(item);
        }

        internal void Clear()
        {
            removed.AddRange(Items);
            items.Clear();
        }

        public event EventHandler AppearanceChanged, PositionChanged, AttributesChanged;
        public event EventHandler<CollectionChangedEventArgs<Item>> ItemsChanged;
        internal void ProcessDelta()
        {
            if (added.Count > 0 || removed.Count > 0)
            {
                ItemsChanged.Raise(new CollectionChangedEventArgs<Item>(added, removed), this);
                added.Clear();
                removed.Clear();
            }

            Delta d = delta;
            Task.Run(() => OnProcessDelta(d));
            delta = Delta.None;
        }

        protected virtual void OnProcessDelta(Delta d)
        {
            if (d.HasFlag(Delta.Appearance))
                AppearanceChanged.Raise(this);

            if (d.HasFlag(Delta.Position))
                PositionChanged.Raise(this);

            if (d.HasFlag(Delta.Attributes))
                AttributesChanged.Raise(this);
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

        public int DistanceTo(Entity entity) { return position.DistanceTo(entity.position); }
        public int Distance { get { return DistanceTo(World.Player); } }
    }
}