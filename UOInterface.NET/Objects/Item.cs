using System;
using System.Text;

namespace UOInterface
{
    public class Item : Entity
    {
        public new static readonly Item Invalid = new Item(Serial.Invalid);
        internal Item(Serial serial) : base(serial) { }

        [Flags]
        protected enum ItemDelta { Ownership }
        protected ItemDelta itemDelta;

        private ushort amount;
        private Serial container;
        private Layer layer;

        public ushort Amount
        {
            get { lock (syncRoot) return amount; }
            internal set
            {
                if (amount != value)
                {
                    amount = value;
                    entityDelta |= EntityDelta.Attributes;
                }
            }
        }

        public Serial Container
        {
            get { lock (syncRoot) return container; }
            internal set
            {
                if (container != value)
                {
                    container = value;
                    itemDelta |= ItemDelta.Ownership;
                }
            }
        }

        public Layer Layer
        {
            get { lock (syncRoot) return layer; }
            internal set
            {
                if (layer != value)
                {
                    layer = value;
                    itemDelta |= ItemDelta.Ownership;
                }
            }
        }

        public event EventHandler OwnerChanged;
        internal override void ProcessDelta()
        {
            base.ProcessDelta();
            if (itemDelta.HasFlag(ItemDelta.Ownership))
                OwnerChanged.RaiseAsync(this);
        }

        protected override void ToString(StringBuilder sb)
        {
            sb.AppendLine();
            sb.AppendFormat("Amount: {0}\n", Amount);
            sb.AppendFormat("Container: {0}\n", Container);
            sb.AppendFormat("Layer: {0}", Layer);
        }

        public override bool IsValid { get { return Serial.IsItem; } }
        public override bool Exists { get { return World.ContainsItem(Serial); } }
        public override int DistanceTo(Entity entity)
        { return OnGround ? base.DistanceTo(entity) : World.GetEntity(RootContainer).DistanceTo(World.Player); }

        public bool OnGround { get { return !Container.IsValid; } }
        public Serial RootContainer
        {
            get
            {
                Item item = this;
                while (item.Container.IsItem)
                    item = World.GetItem(item.Container);
                return item.Container.IsMobile ? item.Container : item;
            }
        }
    }
}