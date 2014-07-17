using System;
using System.Text;

namespace UOInterface
{
    public class Item : Entity
    {
        public new static readonly Item Invalid = new Item(Serial.Invalid);
        internal Item(Serial serial) : base(serial) { }

        public ushort Amount { get; private set; }
        public Serial Container { get; private set; }
        public Layer Layer { get; private set; }
        //public bool Opened { get; private set; }

        protected override void ToString(StringBuilder sb)
        {
            sb.AppendLine();
            sb.AppendFormat("Amount: {0}\n", Amount);
            sb.AppendFormat("Container: {0}\n", Container);
            sb.AppendFormat("Layer: {0}", Layer);
            //sb.AppendFormat("Opened: {0}", Opened);
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

        #region Events
        internal void OnAttributesChanged(UOFlags? flags = null, ushort? amount = null)
        {
            if ((flags.HasValue && flags != Flags) ||
                (amount.HasValue && amount != Amount))
            {
                if (amount.HasValue)
                    Amount = amount.Value;
                base.OnAttributesChanged(flags);
            }
        }

        public event EventHandler OwnerChanged;
        internal void OnOwnerChanged(Serial container, Layer layer = Layer.Invalid)
        {
            if (container != Container || layer != Layer)
            {
                Container = container;
                Layer = layer;
                if (Layer != Layer.Invalid)
                {
                    Mobile m = World.GetMobile(Container);
                    if (m.IsValid)
                        m.OnLayerChanged(Layer, this);
                }
                OwnerChanged.RaiseAsync(this);
            }
        }
        #endregion
    }
}