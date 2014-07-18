using System;
using System.Text;

namespace UOInterface
{
    public class Mobile : Entity
    {
        public new static readonly Mobile Invalid = new Mobile(Serial.Invalid);
        internal Mobile(Serial serial) : base(serial) { }

        [Flags]
        protected enum MobileDelta { Hits, Mana, Stamina, Layer }
        protected MobileDelta mobileDelta;

        private ushort hits;
        private ushort hitsMax;
        private ushort mana;
        private ushort manaMax;
        private ushort stamina;
        private ushort staminaMax;
        private Notoriety notoriety;
        private bool warMode;
        private bool renamable;
        private readonly Serial[] layers = new Serial[0x20];

        public ushort Hits
        {
            get { lock (syncRoot) return hits; }
            internal set
            {
                if (hits != value)
                {
                    hits = value;
                    mobileDelta |= MobileDelta.Hits;
                }
            }
        }

        public ushort HitsMax
        {
            get { lock (syncRoot) return hitsMax; }
            internal set
            {
                if (hitsMax != value)
                {
                    hitsMax = value;
                    mobileDelta |= MobileDelta.Hits;
                }
            }
        }

        public ushort Mana
        {
            get { lock (syncRoot) return mana; }
            internal set
            {
                if (mana != value)
                {
                    mana = value;
                    mobileDelta |= MobileDelta.Mana;
                }
            }
        }

        public ushort ManaMax
        {
            get { lock (syncRoot) return manaMax; }
            internal set
            {
                if (manaMax != value)
                {
                    manaMax = value;
                    mobileDelta |= MobileDelta.Mana;
                }
            }
        }

        public ushort Stamina
        {
            get { lock (syncRoot) return stamina; }
            internal set
            {
                if (stamina != value)
                {
                    stamina = value;
                    mobileDelta |= MobileDelta.Stamina;
                }
            }
        }

        public ushort StaminaMax
        {
            get { lock (syncRoot) return staminaMax; }
            internal set
            {
                if (staminaMax != value)
                {
                    staminaMax = value;
                    mobileDelta |= MobileDelta.Stamina;
                }
            }
        }

        public Notoriety Notoriety
        {
            get { lock (syncRoot) return notoriety; }
            internal set
            {
                if (notoriety != value)
                {
                    notoriety = value;
                    entityDelta |= EntityDelta.Attributes;
                }
            }
        }

        public bool WarMode
        {
            get { lock (syncRoot) return warMode; }
            internal set
            {
                if (warMode != value)
                {
                    warMode = value;
                    entityDelta |= EntityDelta.Attributes;
                }
            }
        }

        public bool Renamable
        {
            get { lock (syncRoot) return renamable; }
            internal set
            {
                if (renamable != value)
                {
                    renamable = value;
                    entityDelta |= EntityDelta.Attributes;
                }
            }
        }

        public Serial this[Layer layer]
        {
            get { lock (syncRoot) return layers[(int)layer]; }
            internal set
            {
                if (layers[(int)layer] != value)
                {
                    layers[(int)layer] = value;
                    mobileDelta |= MobileDelta.Layer;
                }
            }
        }

        public event EventHandler HitsChanged;
        public event EventHandler ManaChanged;
        public event EventHandler StaminaChanged;
        public event EventHandler LayerChanged;
        internal override void ProcessDelta()
        {
            base.ProcessDelta();
            if (mobileDelta.HasFlag(MobileDelta.Hits))
                HitsChanged.RaiseAsync(this);

            if (mobileDelta.HasFlag(MobileDelta.Mana))
                ManaChanged.RaiseAsync(this);

            if (mobileDelta.HasFlag(MobileDelta.Stamina))
                StaminaChanged.RaiseAsync(this);

            if (mobileDelta.HasFlag(MobileDelta.Layer))
                LayerChanged.RaiseAsync(this);
        }

        public override bool IsValid { get { return Serial.IsMobile; } }
        public override bool Exists { get { return World.ContainsMobile(Serial); } }
        protected override void ToString(StringBuilder sb)
        {
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendFormat("Hits: {0}/{1}\n", Hits, HitsMax);
            sb.AppendFormat("Mana: {0}/{1}\n", Mana, ManaMax);
            sb.AppendFormat("Stam: {0}/{1}\n", Stamina, StaminaMax);
            sb.AppendLine();
            sb.AppendFormat("Notoriety: {0}\n", Notoriety);
            sb.AppendFormat("WarMode: {0}\n", WarMode);
            sb.AppendFormat("Renamable: {0}", Renamable);
        }

        public bool YellowBar { get { return Flags.HasFlag(UOFlags.YellowBar); } }
        public bool Poisoned { get { return Flags.HasFlag(UOFlags.Poisoned); } }
        public bool Hidden { get { return Flags.HasFlag(UOFlags.Hidden); } }
        public bool InParty { get { return World.IsInParty(Serial); } }
    }
}