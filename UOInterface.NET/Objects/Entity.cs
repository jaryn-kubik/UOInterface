using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace UOInterface
{
    public class Entity
    {
        protected Entity(Serial serial) { Serial = serial; }

        private Serial serial;
        private Graphic graphic;
        private Hue hue;
        private string name;
        private Position position;
        private Direction direction;
        private UOFlags flags;
        private readonly ConcurrentDictionary<int, UOProperty> properties = new ConcurrentDictionary<int, UOProperty>(1, 16);
        private readonly ConcurrentDictionary<Serial, Item> items = new ConcurrentDictionary<Serial, Item>(1, 32);

        public Serial Serial
        {
            get { return serial; }
            internal set { serial = value; }
        }

        public Graphic Graphic
        {
            get { return graphic; }
            internal set
            {
                if (graphic != value)
                {
                    graphic = value;
                    AddDelta(Delta.Appearance);
                }
            }
        }

        public Hue Hue
        {
            get { return hue; }
            internal set
            {
                if (hue != value)
                {
                    hue = value;
                    AddDelta(Delta.Appearance);
                }
            }
        }

        public string Name
        {
            get { return name; }
            internal set
            {
                if (name != value)
                {
                    name = value;
                    AddDelta(Delta.Appearance);
                }
            }
        }

        public Position Position
        {
            get { return position; }
            internal set
            {
                if (position != value)
                {
                    position = value;
                    AddDelta(Delta.Position);
                }
            }
        }

        public Direction Direction
        {
            get { return direction; }
            internal set
            {
                direction &= Direction.Running;
                if (direction != value)
                {
                    direction = value;
                    AddDelta(Delta.Position);
                }
            }
        }

        public UOFlags Flags
        {
            get { return flags; }
            internal set
            {
                if (flags != value)
                {
                    flags = value;
                    AddDelta(Delta.Attributes);
                }
            }
        }

        public IEnumerable<UOProperty> Properties { get { return properties.Select(p => p.Value); } }
        internal void UpdateProperties(IEnumerable<UOProperty> props)
        {
            properties.Clear();
            int i = 0;
            foreach (UOProperty p in props)
                properties.TryAdd(i++, p);
            AddDelta(Delta.Properties);
        }

        private List<Item> added = new List<Item>(32), removed = new List<Item>(32);
        public IEnumerable<Item> Items { get { return items.Select(item => item.Value); } }
        internal void AddItem(Item item)
        {
            if (items.TryAdd(item.Serial, item))
                lock (syncRoot)
                    added.Add(item);
        }

        internal void RemoveItem(Item item)
        {
            if (items.TryRemove(item.serial, out item))
                lock (syncRoot)
                    removed.Add(item);
        }

        internal void Clear()
        {
            lock (syncRoot)
                removed.AddRange(Items);
            items.Clear();
        }

        public event EventHandler AppearanceChanged, PositionChanged, AttributesChanged, PropertiesChanged;
        public event EventHandler<CollectionChangedEventArgs<Item>> ItemsChanged;
        protected virtual void OnProcessDelta(Delta d)
        {
            if (d.HasFlag(Delta.Appearance))
                AppearanceChanged.Raise(this);

            if (d.HasFlag(Delta.Position))
                PositionChanged.Raise(this);

            if (d.HasFlag(Delta.Attributes))
                AttributesChanged.Raise(this);

            if (d.HasFlag(Delta.Properties))
                PropertiesChanged.Raise(this);
        }

        private static readonly object syncRoot = new object();
        private Delta delta;
        protected void AddDelta(Delta d) { lock (syncRoot)delta |= d; }
        internal void ProcessDelta()
        {
            Delta d;
            CollectionChangedEventArgs<Item> changed = null;
            lock (syncRoot)
            {
                d = delta;
                delta = Delta.None;
                if (added.Count > 0 || removed.Count > 0)
                {
                    changed = new CollectionChangedEventArgs<Item>(added, removed);
                    added = new List<Item>(32);
                    removed = new List<Item>(32);
                }
            }

            if (changed != null)
                ItemsChanged.Raise(changed, this);
            OnProcessDelta(d);
        }

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
            Stats = (1 << 7),
            Skills = (1 << 8),
            Properties = (1 << 9)
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
            sb.AppendLine("\n\nProperties:");
            foreach (UOProperty p in Properties)
            {
                if (string.IsNullOrEmpty(p.Args))
                    sb.AppendFormat("{0}\n", p.Cliloc);
                else
                    sb.AppendFormat("{0} - {1}\n", p.Cliloc, p.Args.Trim());
            }
            return sb.ToString();
        }

        public virtual bool Exists { get { return World.Contains(Serial); } }
        public int DistanceTo(Entity entity) { return position.DistanceTo(entity.position); }
        public int Distance { get { return DistanceTo(World.Player); } }

        public struct UOProperty
        {
            public uint Cliloc { get; private set; }
            public string Args { get; private set; }

            internal UOProperty(uint cliloc, string args)
                : this()
            {
                Cliloc = cliloc;
                Args = args;
            }
        }
    }
}