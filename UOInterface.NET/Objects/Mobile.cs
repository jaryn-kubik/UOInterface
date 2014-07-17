using System;
using System.Text;

namespace UOInterface
{
    public class Mobile : Entity
    {
        public new static readonly Mobile Invalid = new Mobile(Serial.Invalid);
        internal Mobile(Serial serial) : base(serial) { }

        public ushort Hits { get; private set; }
        public ushort HitsMax { get; private set; }
        public ushort Mana { get; private set; }
        public ushort ManaMax { get; private set; }
        public ushort Stamina { get; private set; }
        public ushort StaminaMax { get; private set; }

        public Notoriety Notoriety { get; private set; }
        public bool WarMode { get; private set; }
        public bool Renamable { get; private set; }

        private readonly Serial[] layers = new Serial[0x20];
        public Serial this[Layer layer] { get { return layers[(int)layer]; } }

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

        #region Events
        public event EventHandler HitsChanged;
        internal void OnHitsChanged(ushort hitsMax, ushort hits)
        {
            if (hits != Hits || hitsMax != HitsMax)
            {
                Hits = hits;
                HitsMax = hitsMax;
                HitsChanged.RaiseAsync(this);
            }
        }

        public event EventHandler ManaChanged;
        internal void OnManaChanged(ushort manaMax, ushort mana)
        {
            if (mana != Mana || manaMax != ManaMax)
            {
                Mana = mana;
                ManaMax = manaMax;
                ManaChanged.RaiseAsync(this);
            }
        }

        public event EventHandler StaminaChanged;
        internal void OnStaminaChanged(ushort stamMax, ushort stam)
        {
            if (stam != Stamina || stamMax != StaminaMax)
            {
                Stamina = stam;
                StaminaMax = stamMax;
                StaminaChanged.RaiseAsync(this);
            }
        }

        internal void OnAttributesChanged(UOFlags? flags = null, Notoriety? notoriety = null, bool? warMode = null)
        {
            if ((flags.HasValue && flags != Flags) ||
                (notoriety.HasValue && notoriety != Notoriety) ||
                (warMode.HasValue && warMode != WarMode))
            {
                if (notoriety.HasValue)
                    Notoriety = notoriety.Value;
                if (warMode.HasValue)
                    WarMode = warMode.Value;
                base.OnAttributesChanged(flags);
            }
        }

        internal void OnRenamableChanged(bool renamable)
        {
            if (renamable != Renamable)
            {
                Renamable = renamable;
                base.OnAttributesChanged(null);
            }
        }

        public event EventHandler LayerChanged;
        internal void OnLayerChanged(Layer layer, Serial serial)
        {
            if (serial != layers[(int)layer])
            {
                layers[(int)layer] = serial;
                LayerChanged.RaiseAsync(this);
            }
        }
        #endregion
    }
}