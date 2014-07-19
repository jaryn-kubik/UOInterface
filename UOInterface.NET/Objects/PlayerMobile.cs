using System;
using System.Text;

namespace UOInterface
{
    public class PlayerMobile : Mobile
    {
        public new static readonly PlayerMobile Invalid = new PlayerMobile(Serial.Invalid);
        internal PlayerMobile(Serial serial) : base(serial) { }

        private ushort strength;
        private ushort intelligence;
        private ushort dexterity;
        private ushort weight;
        private ushort weightMax;
        private uint gold;
        private ushort resistPhysical;
        private ushort resistFire;
        private ushort resistCold;
        private ushort resistPoison;
        private ushort resistEnergy;
        private byte followers;
        private byte followersMax;
        private ushort luck;
        private uint tithingPoints;
        private ushort damageMin;
        private ushort damageMax;
        private bool female;

        public ushort Strength
        {
            get { lock (syncRoot) return strength; }
            internal set
            {
                if (strength != value)
                {
                    strength = value;
                    delta |= Delta.Stats;
                }
            }
        }

        public ushort Intelligence
        {
            get { lock (syncRoot) return intelligence; }
            internal set
            {
                if (intelligence != value)
                {
                    intelligence = value;
                    delta |= Delta.Stats;
                }
            }
        }

        public ushort Dexterity
        {
            get { lock (syncRoot) return dexterity; }
            internal set
            {
                if (dexterity != value)
                {
                    dexterity = value;
                    delta |= Delta.Stats;
                }
            }
        }

        public ushort Weight
        {
            get { lock (syncRoot) return weight; }
            internal set
            {
                if (weight != value)
                {
                    weight = value;
                    delta |= Delta.Stats;
                }
            }
        }

        public ushort WeightMax
        {
            get { lock (syncRoot) return weightMax; }
            internal set
            {
                if (weightMax != value)
                {
                    weightMax = value;
                    delta |= Delta.Stats;
                }
            }
        }

        public uint Gold
        {
            get { lock (syncRoot) return gold; }
            internal set
            {
                if (gold != value)
                {
                    gold = value;
                    delta |= Delta.Stats;
                }
            }
        }

        public ushort ResistPhysical
        {
            get { lock (syncRoot) return resistPhysical; }
            internal set
            {
                if (resistPhysical != value)
                {
                    resistPhysical = value;
                    delta |= Delta.Stats;
                }
            }
        }

        public ushort ResistFire
        {
            get { lock (syncRoot) return resistFire; }
            internal set
            {
                if (resistFire != value)
                {
                    resistFire = value;
                    delta |= Delta.Stats;
                }
            }
        }

        public ushort ResistCold
        {
            get { lock (syncRoot) return resistCold; }
            internal set
            {
                if (resistCold != value)
                {
                    resistCold = value;
                    delta |= Delta.Stats;
                }
            }
        }

        public ushort ResistPoison
        {
            get { lock (syncRoot) return resistPoison; }
            internal set
            {
                if (resistPoison != value)
                {
                    resistPoison = value;
                    delta |= Delta.Stats;
                }
            }
        }

        public ushort ResistEnergy
        {
            get { lock (syncRoot) return resistEnergy; }
            internal set
            {
                if (resistEnergy != value)
                {
                    resistEnergy = value;
                    delta |= Delta.Stats;
                }
            }
        }

        public byte Followers
        {
            get { lock (syncRoot) return followers; }
            internal set
            {
                if (followers != value)
                {
                    followers = value;
                    delta |= Delta.Stats;
                }
            }
        }

        public byte FollowersMax
        {
            get { lock (syncRoot) return followersMax; }
            internal set
            {
                if (followersMax != value)
                {
                    followersMax = value;
                    delta |= Delta.Stats;
                }
            }
        }

        public ushort Luck
        {
            get { lock (syncRoot) return luck; }
            internal set
            {
                if (luck != value)
                {
                    luck = value;
                    delta |= Delta.Stats;
                }
            }
        }

        public uint TithingPoints
        {
            get { lock (syncRoot) return tithingPoints; }
            internal set
            {
                if (tithingPoints != value)
                {
                    tithingPoints = value;
                    delta |= Delta.Stats;
                }
            }
        }

        public ushort DamageMin
        {
            get { lock (syncRoot) return damageMin; }
            internal set
            {
                if (damageMin != value)
                {
                    damageMin = value;
                    delta |= Delta.Stats;
                }
            }
        }

        public ushort DamageMax
        {
            get { lock (syncRoot) return damageMax; }
            internal set
            {
                if (damageMax != value)
                {
                    damageMax = value;
                    delta |= Delta.Stats;
                }
            }
        }

        public bool Female
        {
            get { lock (syncRoot) return female; }
            internal set
            {
                if (female != value)
                {
                    female = value;
                    delta |= Delta.Stats;
                }
            }
        }

        public event EventHandler StatsChanged;
        protected override void OnProcessDelta(Delta d)
        {
            base.OnProcessDelta(d);
            if (d.HasFlag(Delta.Stats))
                StatsChanged.Raise(this);
        }

        protected override void ToString(StringBuilder sb)
        {
            base.ToString(sb);
            sb.AppendLine();
            sb.AppendLine();
            sb.AppendFormat("Str: {0}\n", Strength);
            sb.AppendFormat("Int: {0}\n", Intelligence);
            sb.AppendFormat("Dex: {0}\n", Dexterity);
            sb.AppendFormat("Weight: {0}/{1}\n", Weight, WeightMax);
            sb.AppendFormat("Gold: {0}\n", Gold);
            sb.AppendFormat("Resists (Ph,F,C,Po,E): {0}, {1}, {2}, {3}, {4}\n", ResistPhysical, ResistFire, ResistCold, ResistPoison, ResistEnergy);
            sb.AppendLine();
            sb.AppendFormat("Foll: {0}/{1}\n", Followers, FollowersMax);
            sb.AppendFormat("Luck: {0}\n", Luck);
            sb.AppendFormat("Tiths: {0}\n", TithingPoints);
            sb.AppendFormat("Damage: {0}-{1}\n", DamageMin, DamageMax);
            sb.AppendFormat("Female: {0}", Female);
        }
    }
}