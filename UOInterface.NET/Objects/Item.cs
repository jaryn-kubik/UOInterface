using System.Text;

namespace UOInterface
{
    public class Item : Entity
    {
        public new static readonly Item Invalid = new Item(Serial.Invalid);
        internal Item(Serial serial) : base(serial) { }

        public ushort Amount { get; internal set; }
        public Layer Layer { get; internal set; }
        public Serial Container { get; internal set; }
        public bool Opened { get; internal set; }

        protected override void ToString(StringBuilder sb)
        {
            sb.AppendFormat("Amount: {0}\n", Amount);
            sb.AppendFormat("Layer: {0}\n", Amount);
            sb.AppendFormat("Container: {0}\n", Container);
            sb.AppendFormat("Opened: {0}\n", Opened);
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