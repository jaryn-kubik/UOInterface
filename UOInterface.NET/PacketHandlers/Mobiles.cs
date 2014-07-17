using System;
using UOInterface.Network;

namespace UOInterface
{
    public static partial class World
    {
        private static void OnMobileMoving(Packet p)//0x77
        {
            Mobile mobile = GetOrCreateMobile(p.ReadUInt());
            ushort g = p.ReadUShort();
            mobile.OnMoved(new Position(p.ReadUShort(), p.ReadUShort(), p.ReadSByte()), (Direction)p.ReadByte());
            mobile.OnAppearanceChanged(g, p.ReadUShort());
            mobile.OnAttributesChanged((UOFlags)p.ReadByte(), (Notoriety)p.ReadByte());
            AddMobile(mobile);
        }

        private static void OnMobileIncoming(Packet p)//0x78
        {
            Mobile mobile = GetOrCreateMobile(p.ReadUInt());
            ushort g = p.ReadUShort();
            mobile.OnMoved(new Position(p.ReadUShort(), p.ReadUShort(), p.ReadSByte()), (Direction)p.ReadByte());
            mobile.OnAppearanceChanged(g, p.ReadUShort());
            mobile.OnAttributesChanged((UOFlags)p.ReadByte(), (Notoriety)p.ReadByte());

            uint itemSerial;
            while ((itemSerial = p.ReadUInt()) != 0)
            {
                Item item = GetOrCreateItem(itemSerial);
                ushort graphic = p.ReadUShort();
                Layer layer = (Layer)p.ReadByte();
                ushort hue = item.Hue;
                if (useNewMobileIncoming.Value || (graphic & 0x8000) != 0)
                    hue = p.ReadUShort();

                if (!useNewMobileIncoming.Value)
                {
                    if (usePostSAChanges.Value)
                        graphic &= 0x7FFF;
                    else
                        graphic &= 0x3FFF;
                }

                item.OnAppearanceChanged(graphic, hue);
                item.OnOwnerChanged(mobile.Serial, layer);
                mobile.OnLayerChanged(layer, item.Serial);
                AddItem(item);
            }

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
                throw new Exception("OnMobileStatus - !mobile.IsValid");//does this happen?

            mobile.OnAppearanceChanged(name: p.ReadStringAscii(30));
            mobile.OnHitsChanged(hits: p.ReadUShort(), hitsMax: p.ReadUShort());
            mobile.OnRenamableChanged(p.ReadBool());

            byte type = p.ReadByte();
            if (type > 0)
            {
                if (mobile != Player)
                    throw new Exception("OnMobileStatus - mobile != Player");//does this happen?

                Player.Female = p.ReadBool();
                Player.Strength = p.ReadUShort();
                Player.Dexterity = p.ReadUShort();
                Player.Intelligence = p.ReadUShort();
                Player.OnStaminaChanged(stam: p.ReadUShort(), stamMax: p.ReadUShort());
                Player.OnManaChanged(mana: p.ReadUShort(), manaMax: p.ReadUShort());
                Player.Gold = p.ReadUInt();
                Player.ResistPhysical = p.ReadUShort();
                Player.Weight = p.ReadUShort();
            }

            if (type >= 5)//ML
            {
                Player.WeightMax = p.ReadUShort();
                p.Skip(1);
            }

            if (type >= 2)//T2A
                p.Skip(2);

            if (type >= 3)//Renaissance
            {
                Player.Followers = p.ReadByte();
                Player.FollowersMax = p.ReadByte();
            }

            if (type >= 4)//AOS
            {
                Player.ResistFire = p.ReadUShort();
                Player.ResistCold = p.ReadUShort();
                Player.ResistPoison = p.ReadUShort();
                Player.ResistEnergy = p.ReadUShort();
                Player.Luck = p.ReadUShort();
                Player.DamageMin = p.ReadUShort();
                Player.DamageMax = p.ReadUShort();
                Player.TithingPoints = p.ReadUInt();
            }
            Player.OnStatsChanged();
        }

        private static void OnMobileHealthbar(Packet p)//0x17
        {
            Mobile mobile = GetMobile(p.ReadUInt());
            if (!mobile.IsValid)
                return;

            p.Skip(2);//unknown

            UOFlags flag;
            ushort type = p.ReadUShort();
            if (type == 1)
                flag = UOFlags.Poisoned;
            else if (type == 2)
                flag = UOFlags.YellowBar;
            else
                return;

            UOFlags flags = p.ReadBool() ? mobile.Flags | flag : mobile.Flags & ~flag;
            mobile.OnAttributesChanged(flags);
        }
    }
}