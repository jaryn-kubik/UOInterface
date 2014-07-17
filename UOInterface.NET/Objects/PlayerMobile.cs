using System;
using System.Text;

namespace UOInterface
{
    public class PlayerMobile : Mobile
    {
        public new static readonly PlayerMobile Invalid = new PlayerMobile(Serial.Invalid);
        internal PlayerMobile(Serial serial) : base(serial) { }

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
        public bool Female { get; internal set; }

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

        public event EventHandler StatsChanged;
        internal void OnStatsChanged() { StatsChanged.RaiseAsync(this); }
    }
}