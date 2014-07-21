using System;
using System.Collections.Generic;
using System.Text;

namespace UOInterface
{
    public class PlayerMobile : Mobile
    {
        private static readonly int skillCount = Enum.GetValues(typeof(SkillName)).Length;
        public new static readonly PlayerMobile Invalid = new PlayerMobile(Serial.Invalid);
        internal PlayerMobile(Serial serial)
            : base(serial)
        {
            for (int i = 0; i < skills.Length; i++)
                skills[i] = new Skill();
        }

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
        private readonly Skill[] skills = new Skill[skillCount];

        public ushort Strength
        {
            get { return strength; }
            internal set
            {
                if (strength != value)
                {
                    strength = value;
                    AddDelta(Delta.Stats);
                }
            }
        }

        public ushort Intelligence
        {
            get { return intelligence; }
            internal set
            {
                if (intelligence != value)
                {
                    intelligence = value;
                    AddDelta(Delta.Stats);
                }
            }
        }

        public ushort Dexterity
        {
            get { return dexterity; }
            internal set
            {
                if (dexterity != value)
                {
                    dexterity = value;
                    AddDelta(Delta.Stats);
                }
            }
        }

        public ushort Weight
        {
            get { return weight; }
            internal set
            {
                if (weight != value)
                {
                    weight = value;
                    AddDelta(Delta.Stats);
                }
            }
        }

        public ushort WeightMax
        {
            get { return weightMax; }
            internal set
            {
                if (weightMax != value)
                {
                    weightMax = value;
                    AddDelta(Delta.Stats);
                }
            }
        }

        public uint Gold
        {
            get { return gold; }
            internal set
            {
                if (gold != value)
                {
                    gold = value;
                    AddDelta(Delta.Stats);
                }
            }
        }

        public ushort ResistPhysical
        {
            get { return resistPhysical; }
            internal set
            {
                if (resistPhysical != value)
                {
                    resistPhysical = value;
                    AddDelta(Delta.Stats);
                }
            }
        }

        public ushort ResistFire
        {
            get { return resistFire; }
            internal set
            {
                if (resistFire != value)
                {
                    resistFire = value;
                    AddDelta(Delta.Stats);
                }
            }
        }

        public ushort ResistCold
        {
            get { return resistCold; }
            internal set
            {
                if (resistCold != value)
                {
                    resistCold = value;
                    AddDelta(Delta.Stats);
                }
            }
        }

        public ushort ResistPoison
        {
            get { return resistPoison; }
            internal set
            {
                if (resistPoison != value)
                {
                    resistPoison = value;
                    AddDelta(Delta.Stats);
                }
            }
        }

        public ushort ResistEnergy
        {
            get { return resistEnergy; }
            internal set
            {
                if (resistEnergy != value)
                {
                    resistEnergy = value;
                    AddDelta(Delta.Stats);
                }
            }
        }

        public byte Followers
        {
            get { return followers; }
            internal set
            {
                if (followers != value)
                {
                    followers = value;
                    AddDelta(Delta.Stats);
                }
            }
        }

        public byte FollowersMax
        {
            get { return followersMax; }
            internal set
            {
                if (followersMax != value)
                {
                    followersMax = value;
                    AddDelta(Delta.Stats);
                }
            }
        }

        public ushort Luck
        {
            get { return luck; }
            internal set
            {
                if (luck != value)
                {
                    luck = value;
                    AddDelta(Delta.Stats);
                }
            }
        }

        public uint TithingPoints
        {
            get { return tithingPoints; }
            internal set
            {
                if (tithingPoints != value)
                {
                    tithingPoints = value;
                    AddDelta(Delta.Stats);
                }
            }
        }

        public ushort DamageMin
        {
            get { return damageMin; }
            internal set
            {
                if (damageMin != value)
                {
                    damageMin = value;
                    AddDelta(Delta.Stats);
                }
            }
        }

        public ushort DamageMax
        {
            get { return damageMax; }
            internal set
            {
                if (damageMax != value)
                {
                    damageMax = value;
                    AddDelta(Delta.Stats);
                }
            }
        }

        public bool Female
        {
            get { return female; }
            internal set
            {
                if (female != value)
                {
                    female = value;
                    AddDelta(Delta.Stats);
                }
            }
        }

        public IReadOnlyList<Skill> Skills { get { return skills; } }
        internal void UpdateSkill(int id, ushort realValue, ushort baseValue, SkillLock skillLock, ushort cap)
        {
            if (id < skills.Length)
            {
                skills[id].ValueFixed = realValue;
                skills[id].BaseFixed = baseValue;
                skills[id].Lock = skillLock;
                skills[id].CapFixed = cap;
                AddDelta(Delta.Skills);
            }
        }

        internal void UpdateSkillLock(int id, SkillLock skillLock)
        {
            if (id < skills.Length)
            {
                skills[id].Lock = skillLock;
                AddDelta(Delta.Skills);
            }
        }

        public event EventHandler StatsChanged, SkillsChanged;
        protected override void OnProcessDelta(Delta d)
        {
            base.OnProcessDelta(d);
            if (d.HasFlag(Delta.Stats))
                StatsChanged.Raise(this);

            if (d.HasFlag(Delta.Skills))
                SkillsChanged.Raise(this);
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

        public class Skill
        {
            public SkillLock Lock { get; internal set; }
            public ushort ValueFixed { get; internal set; }
            public ushort BaseFixed { get; internal set; }
            public ushort CapFixed { get; internal set; }

            public double Value { get { return ValueFixed / 10.0; } }
            public double Base { get { return BaseFixed / 10.0; } }
            public double Cap { get { return CapFixed / 10.0; } }
        }
    }
}