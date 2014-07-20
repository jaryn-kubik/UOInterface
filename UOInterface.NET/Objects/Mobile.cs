using System;
using System.Text;

namespace UOInterface
{
    public class Mobile : Entity
    {
        public new static readonly Mobile Invalid = new Mobile(Serial.Invalid);
        internal Mobile(Serial serial) : base(serial) { }

        private ushort hits;
        private ushort hitsMax;
        private ushort mana;
        private ushort manaMax;
        private ushort stamina;
        private ushort staminaMax;
        private Notoriety notoriety;
        private bool warMode;
        private bool renamable;

        public ushort Hits
        {
            get { return hits; }
            internal set
            {
                if (hits != value)
                {
                    hits = value;
                    delta |= Delta.Hits;
                }
            }
        }

        public ushort HitsMax
        {
            get { return hitsMax; }
            internal set
            {
                if (hitsMax != value)
                {
                    hitsMax = value;
                    delta |= Delta.Hits;
                }
            }
        }

        public ushort Mana
        {
            get { return mana; }
            internal set
            {
                if (mana != value)
                {
                    mana = value;
                    delta |= Delta.Mana;
                }
            }
        }

        public ushort ManaMax
        {
            get { return manaMax; }
            internal set
            {
                if (manaMax != value)
                {
                    manaMax = value;
                    delta |= Delta.Mana;
                }
            }
        }

        public ushort Stamina
        {
            get { return stamina; }
            internal set
            {
                if (stamina != value)
                {
                    stamina = value;
                    delta |= Delta.Stamina;
                }
            }
        }

        public ushort StaminaMax
        {
            get { return staminaMax; }
            internal set
            {
                if (staminaMax != value)
                {
                    staminaMax = value;
                    delta |= Delta.Stamina;
                }
            }
        }

        public Notoriety Notoriety
        {
            get { return notoriety; }
            internal set
            {
                if (notoriety != value)
                {
                    notoriety = value;
                    delta |= Delta.Attributes;
                }
            }
        }

        public bool WarMode
        {
            get { return warMode; }
            internal set
            {
                if (warMode != value)
                {
                    warMode = value;
                    delta |= Delta.Attributes;
                }
            }
        }

        public bool Renamable
        {
            get { return renamable; }
            internal set
            {
                if (renamable != value)
                {
                    renamable = value;
                    delta |= Delta.Attributes;
                }
            }
        }

        public event EventHandler HitsChanged;
        public event EventHandler ManaChanged;
        public event EventHandler StaminaChanged;
        protected override void OnProcessDelta(Delta d)
        {
            base.OnProcessDelta(d);
            if (d.HasFlag(Delta.Hits))
                HitsChanged.Raise(this);

            if (d.HasFlag(Delta.Mana))
                ManaChanged.Raise(this);

            if (d.HasFlag(Delta.Stamina))
                StaminaChanged.Raise(this);
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