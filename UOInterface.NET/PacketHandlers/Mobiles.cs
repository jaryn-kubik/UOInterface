using System;
using UOInterface.Network;

namespace UOInterface
{
    public static partial class World
    {
        private static void OnMobileMoving(Packet p)//0x77
        {
            Mobile mobile = GetOrCreateMobile(p.ReadUInt());
            mobile.Graphic = p.ReadUShort();
            ushort x = p.ReadUShort();
            ushort y = p.ReadUShort();
            sbyte z = p.ReadSByte();
            mobile.Direction = (Direction)p.ReadByte() & ~Direction.Running;
            mobile.Hue = p.ReadUShort();
            mobile.OnFlagsChanged((MobileFlags)p.ReadByte());
            mobile.Notoriety = (Notoriety)p.ReadByte();

            mobile.OnPositionChanged(new Position(x, y, z));
            AddMobile(mobile);
        }

        private static void OnMobileIncoming(Packet p)//0x78
        {
            Mobile mobile = GetOrCreateMobile(p.ReadUInt());
            mobile.Graphic = p.ReadUShort();
            ushort x = p.ReadUShort();
            ushort y = p.ReadUShort();
            sbyte z = p.ReadSByte();
            mobile.Direction = (Direction)p.ReadByte() & ~Direction.Running;
            mobile.Hue = p.ReadUShort();
            mobile.OnFlagsChanged((MobileFlags)p.ReadByte());
            mobile.Notoriety = (Notoriety)p.ReadByte();

            uint itemSerial;
            while ((itemSerial = p.ReadUInt()) != 0)
            {
                Item item = GetOrCreateItem(itemSerial);
                ushort graphic = p.ReadUShort();

                if (useNewMobileIncoming.Value)
                    item.Graphic = graphic;
                else if (usePostSAChanges.Value)
                    item.Graphic = (ushort)(graphic & 0x7FFF);
                else
                    item.Graphic = (ushort)(graphic & 0x3FFF);

                item.Layer = (Layer)p.ReadByte();

                if (useNewMobileIncoming.Value || (graphic & 0x8000) != 0)
                    item.Hue = p.ReadUShort();

                item.Container = mobile.Serial;
                mobile[item.Layer] = item.Serial;
                AddItem(item);
            }

            mobile.OnPositionChanged(new Position(x, y, z));
            AddMobile(mobile);
        }

        private static void OnMobileAttributes(Packet p)//0x2D
        {
            Mobile mobile = GetMobile(p.ReadUInt());
            if (mobile.IsValid)
            {
                mobile.OnHitsChanged(p.ReadUShort(), p.ReadUShort());
                mobile.OnManaChanged(p.ReadUShort(), p.ReadUShort());
                mobile.OnStaminaChanged(p.ReadUShort(), p.ReadUShort());
            }
        }

        private static void OnMobileHits(Packet p)//0xA1
        {
            Mobile mobile = GetMobile(p.ReadUInt());
            if (mobile.IsValid)
                mobile.OnHitsChanged(p.ReadUShort(), p.ReadUShort());
        }

        private static void OnMobileMana(Packet p)//0xA2
        {
            Mobile mobile = GetMobile(p.ReadUInt());
            if (mobile.IsValid)
                mobile.OnManaChanged(p.ReadUShort(), p.ReadUShort());
        }

        private static void OnMobileStamina(Packet p)//0xA3
        {
            Mobile mobile = GetMobile(p.ReadUInt());
            if (mobile.IsValid)
                mobile.OnStaminaChanged(p.ReadUShort(), p.ReadUShort());
        }

        private static void OnMobileStatus(Packet p)//0x11
        {
            Mobile mobile = GetMobile(p.ReadUInt());
            if (!mobile.IsValid)
                throw new Exception("OnMobileStatus");//does this happen?

            mobile.Name = p.ReadStringAscii(30);
            mobile.OnHitsChanged(hits: p.ReadUShort(), hitsMax: p.ReadUShort());
            mobile.Renamable = p.ReadBool();

            byte type = p.ReadByte();
            if (type > 0)
            {
                mobile.Female = p.ReadBool();
                mobile.Strength = p.ReadUShort();
                mobile.Dexterity = p.ReadUShort();
                mobile.Intelligence = p.ReadUShort();
                mobile.OnStaminaChanged(stam: p.ReadUShort(), stamMax: p.ReadUShort());
                mobile.OnManaChanged(mana: p.ReadUShort(), manaMax: p.ReadUShort());
                mobile.Gold = p.ReadUInt();
                mobile.ResistPhysical = p.ReadUShort();
                mobile.Weight = p.ReadUShort();
            }

            if (type >= 5)//ML
            {
                mobile.WeightMax = p.ReadUShort();
                p.Skip(1);
            }

            if (type >= 2)//T2A
                p.Skip(2);

            if (type >= 3)//Renaissance
            {
                mobile.Followers = p.ReadByte();
                mobile.FollowersMax = p.ReadByte();
            }

            if (type >= 4)//AOS
            {
                mobile.ResistFire = p.ReadUShort();
                mobile.ResistCold = p.ReadUShort();
                mobile.ResistPoison = p.ReadUShort();
                mobile.ResistEnergy = p.ReadUShort();
                mobile.Luck = p.ReadUShort();
                mobile.DamageMin = p.ReadUShort();
                mobile.DamageMax = p.ReadUShort();
                mobile.TithingPoints = p.ReadUInt();
            }
            mobile.OnStatusChanged();
        }
    }
}