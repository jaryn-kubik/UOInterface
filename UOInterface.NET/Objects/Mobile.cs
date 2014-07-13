using System.Text;

namespace UOInterface
{
    public class Mobile : Entity
    {
        public new static readonly Mobile Invalid = new Mobile(Serial.Invalid);
        internal Mobile(Serial serial) : base(serial) { Layers = new Serial[0x20]; }

        public ushort Hits { get; internal set; }
        public ushort HitsMax { get; internal set; }
        public ushort Mana { get; internal set; }
        public ushort ManaMax { get; internal set; }
        public ushort Stamina { get; internal set; }
        public ushort StaminaMax { get; internal set; }

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

        public Direction Direction { get; internal set; }
        public Notoriety Notoriety { get; internal set; }
        public bool WarMode { get; internal set; }
        public bool Renamable { get; internal set; }
        public bool Female { get; internal set; }

        public Serial[] Layers { get; internal set; }

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
            sb.AppendFormat("Direction: {0}\n", Direction);
            sb.AppendFormat("Notoriety: {0}\n", Notoriety);
            sb.AppendFormat("WarMode: {0}\n", WarMode);
            sb.AppendFormat("Renamable: {0}\n", Renamable);
            sb.AppendFormat("Female: {0}", Female);
        }

        public override bool IsValid { get { return Serial.IsMobile; } }
        public override bool Exists { get { return World.ContainsMobile(Serial); } }
        public int Distance { get { return Position.DistanceTo(World.Player.Position); } }
        public bool YellowBar { get { return Flags.HasFlag(UOFlags.YellowBar); } }
        public bool Poisoned { get { return Flags.HasFlag(UOFlags.Poisoned); } }
        public bool Hidden { get { return Flags.HasFlag(UOFlags.Hidden); } }
        //public bool InParty { get { return World.IsInParty(Serial); } }
    }
}