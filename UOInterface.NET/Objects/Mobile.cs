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

        public MobileFlags Flags { get; private set; }
        public Direction Direction { get; private set; }
        public Notoriety Notoriety { get; private set; }
        public bool WarMode { get; private set; }

        public ushort Strength { get; internal set; }
        public ushort Intelligence { get; internal set; }
        public ushort Dexterity { get; internal set; }
        public ushort Weight { get; internal set; }
        public ushort WeightMax { get; internal set; }
        public uint Gold { get; internal set; }

        public ushort ResistPhysical { get; internal set; }
        public ushort ResistFire { get; internal set; }
        public ushort ResistCold { get; internal set; }
        public ushort ResistPoison { get; internal set; }
        public ushort ResistEnergy { get; internal set; }

        public byte Followers { get; internal set; }
        public byte FollowersMax { get; internal set; }
        public ushort Luck { get; internal set; }
        public uint TithingPoints { get; internal set; }

        public ushort DamageMin { get; internal set; }
        public ushort DamageMax { get; internal set; }

        public bool Renamable { get; internal set; }
        public bool Female { get; internal set; }

        private readonly Serial[] layers = new Serial[0x20];
        public Serial this[Layer layer] { get { return layers[(int)layer]; } }

        public override bool IsValid { get { return Serial.IsMobile; } }
        public override bool Exists { get { return World.ContainsMobile(Serial); } }
        protected override void ToString(StringBuilder sb)
        {
            sb.AppendFormat("Hits: {0}/{1}\n", Hits, HitsMax);
            sb.AppendFormat("Mana: {0}/{1}\n", Mana, ManaMax);
            sb.AppendFormat("Stam: {0}/{1}\n", Stamina, StaminaMax);
            sb.AppendFormat("Str: {0}\n", Strength);
            sb.AppendFormat("Int: {0}\n", Intelligence);
            sb.AppendFormat("Dex: {0}\n", Dexterity);
            sb.AppendLine();
            sb.AppendFormat("Weight: {0}/{1}\n", Weight, WeightMax);
            sb.AppendFormat("Gold: {0}\n", Gold);
            sb.AppendFormat("Resists (Ph,F,C,Po,E): {0}, {1}, {2}, {3}, {4}\n", ResistPhysical, ResistFire, ResistCold, ResistPoison, ResistEnergy);
            sb.AppendLine();
            sb.AppendFormat("Foll: {0}/{1}\n", Followers, FollowersMax);
            sb.AppendFormat("Luck: {0}\n", Luck);
            sb.AppendFormat("Tiths: {0}\n", TithingPoints);
            sb.AppendFormat("Damage: {0}-{1}\n", DamageMin, DamageMax);
            sb.AppendLine();
            sb.AppendFormat("Flags: {0}\n", Flags);
            sb.AppendFormat("Direction: {0}\n", Direction);
            sb.AppendFormat("Notoriety: {0}\n", Notoriety);
            sb.AppendFormat("WarMode: {0}\n", WarMode);
            sb.AppendFormat("Renamable: {0}\n", Renamable);
            sb.AppendFormat("Female: {0}", Female);
        }

        public bool YellowBar { get { return Flags.HasFlag(MobileFlags.YellowBar); } }
        public bool Poisoned { get { return Flags.HasFlag(MobileFlags.Poisoned); } }
        public bool Hidden { get { return Flags.HasFlag(MobileFlags.Hidden); } }
        public bool InParty { get { return World.IsInParty(Serial); } }

        #region Events
        public event EventHandler Moved;
        internal void OnMoved(Position position, Direction dir)
        {
            dir &= ~Direction.Running;
            if (position != Position || dir != Direction)
            {
                Position = position;
                Direction = dir;
                Moved.RaiseAsync(this);
            }
        }

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

        public event EventHandler FlagsChanged;
        internal void OnFlagsChanged(MobileFlags? flags = null, Notoriety? notoriety = null, bool? warMode = null)
        {
            if ((flags.HasValue && flags != Flags) ||
                (notoriety.HasValue && notoriety != Notoriety) ||
                (warMode.HasValue && warMode != WarMode))
            {
                if (flags.HasValue)
                    Flags = flags.Value;
                if (notoriety.HasValue)
                    Notoriety = notoriety.Value;
                if (warMode.HasValue)
                    WarMode = warMode.Value;
                FlagsChanged.RaiseAsync(this);
            }
        }

        public event EventHandler StatusChanged;
        internal void OnStatusChanged() { StatusChanged.RaiseAsync(this); }

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