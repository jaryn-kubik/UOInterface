using System;
using System.Text;

namespace UOInterface
{
    public class Item : Entity
    {
        internal Item(Serial serial) : base(serial) { }

        private ushort amount;
        private Serial container;
        private Layer layer;

        public ushort Amount
        {
            get { return amount; }
            internal set
            {
                if (amount != value)
                {
                    amount = value;
                    AddDelta(Delta.Attributes);
                }
            }
        }

        public Serial Container
        {
            get { return container; }
            internal set
            {
                if (container != value)
                {
                    container = value;
                    AddDelta(Delta.Ownership);
                }
            }
        }

        public Layer Layer
        {
            get { return layer; }
            internal set
            {
                if (layer != value)
                {
                    layer = value;
                    AddDelta(Delta.Ownership);
                }
            }
        }

        public event EventHandler OwnerChanged;
        protected override void OnProcessDelta(Delta d)
        {
            base.OnProcessDelta(d);
            if (d.HasFlag(Delta.Ownership))
                OwnerChanged.Raise(this);
        }

        protected override void ToString(StringBuilder sb)
        {
            sb.AppendLine();
            sb.AppendFormat("Amount: {0}\n", Amount);
            sb.AppendFormat("Container: {0}\n", Container);
            sb.AppendFormat("Layer: {0}", Layer);
        }

        public override bool Exists { get { return World.ContainsItem(Serial); } }
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