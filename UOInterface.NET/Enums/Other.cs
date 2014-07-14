using System;

namespace UOInterface
{
    public enum Map : byte
    {
        Felucca,
        Trammel,
        Ilshenar,
        Malas,
        Tokuno,
        TerMur
    }

    [Flags]
    public enum UOFlags : byte
    {
        Frozen = 0x01,
        Female = 0x02,
        Poisoned = 0x04,
        YellowBar = 0x08,
        WarMode = 0x40,
        Hidden = 0x80
    }

    [Flags]
    public enum Direction : byte
    {
        North = 0x00,
        Right = 0x01,
        East = 0x02,
        Down = 0x03,
        South = 0x04,
        Left = 0x05,
        West = 0x06,
        Up = 0x07,
        Running = 0x80
    }

    public enum Notoriety : byte
    {
        Unknown = 0x00,
        Innocent = 0x01,
        Ally = 0x02,
        Gray = 0x03,
        Criminal = 0x04,
        Enemy = 0x05,
        Murderer = 0x06,
        Invulnerable = 0x07
    }

    public enum Virtue : ushort
    {
        Honor = 1,
        Sacrifice,
        Valor,
        Compassion,
        Justice = 7
    }
}