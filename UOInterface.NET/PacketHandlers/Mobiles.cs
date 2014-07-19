﻿using System;
using UOInterface.Network;

namespace UOInterface
{
    public static partial class World
    {
        private static void OnMobileMoving(Packet p)//0x77
        {
            Mobile mobile = GetOrCreateMobile(p.ReadUInt());
            lock (mobile.SyncRoot)
            {
                mobile.Graphic = p.ReadUShort();
                mobile.Position = new Position(p.ReadUShort(), p.ReadUShort(), p.ReadSByte());
                mobile.Direction = (Direction)p.ReadByte();
                mobile.Hue = p.ReadUShort();
                mobile.Flags = (UOFlags)p.ReadByte();
                mobile.Notoriety = (Notoriety)p.ReadByte();
            }
            mobile.ProcessDelta();
            ProcessDelta();
        }

        private static void OnMobileIncoming(Packet p)//0x78
        {
            Mobile mobile = GetOrCreateMobile(p.ReadUInt());
            lock (mobile.SyncRoot)
            {
                mobile.Graphic = p.ReadUShort();
                mobile.Position = new Position(p.ReadUShort(), p.ReadUShort(), p.ReadSByte());
                mobile.Direction = (Direction)p.ReadByte();
                mobile.Hue = p.ReadUShort();
                mobile.Flags = (UOFlags)p.ReadByte();
                mobile.Notoriety = (Notoriety)p.ReadByte();

                uint itemSerial;
                while ((itemSerial = p.ReadUInt()) != 0)
                {
                    Item item = GetOrCreateItem(itemSerial);
                    lock (item.SyncRoot)
                    {
                        ushort graphic = p.ReadUShort();
                        item.Layer = (Layer)p.ReadByte();
                        if (useNewMobileIncoming.Value || (graphic & 0x8000) != 0)
                            item.Hue = p.ReadUShort();

                        if (useNewMobileIncoming.Value)
                            item.Graphic = graphic;
                        else if (usePostSAChanges.Value)
                            item.Graphic = (ushort)(graphic & 0x7FFF);
                        else
                            item.Graphic = (ushort)(graphic & 0x3FFF);

                        item.Container = mobile;
                        mobile.AddItem(item);
                    }
                    item.ProcessDelta();
                }
            }
            mobile.ProcessDelta();
            ProcessDelta();
        }

        private static void OnMobileAttributes(Packet p)//0x2D
        {
            Mobile mobile = GetMobile(p.ReadUInt());
            if (!mobile.IsValid)
                return;
            lock (mobile.SyncRoot)
            {
                mobile.HitsMax = p.ReadUShort();
                mobile.Hits = p.ReadUShort();
                mobile.ManaMax = p.ReadUShort();
                mobile.Mana = p.ReadUShort();
                mobile.StaminaMax = p.ReadUShort();
                mobile.Stamina = p.ReadUShort();
            }
            mobile.ProcessDelta();
        }

        private static void OnMobileHits(Packet p)//0xA1
        {
            Mobile mobile = GetMobile(p.ReadUInt());
            if (!mobile.IsValid)
                return;
            lock (mobile.SyncRoot)
            {
                mobile.HitsMax = p.ReadUShort();
                mobile.Hits = p.ReadUShort();
            }
            mobile.ProcessDelta();
        }

        private static void OnMobileMana(Packet p)//0xA2
        {
            Mobile mobile = GetMobile(p.ReadUInt());
            if (!mobile.IsValid)
                return;
            lock (mobile.SyncRoot)
            {
                mobile.ManaMax = p.ReadUShort();
                mobile.Mana = p.ReadUShort();
            }
            mobile.ProcessDelta();
        }

        private static void OnMobileStamina(Packet p)//0xA3
        {
            Mobile mobile = GetMobile(p.ReadUInt());
            if (!mobile.IsValid)
                return;
            lock (mobile.SyncRoot)
            {
                mobile.StaminaMax = p.ReadUShort();
                mobile.Stamina = p.ReadUShort();
            }
            mobile.ProcessDelta();
        }

        private static void OnMobileStatus(Packet p)//0x11
        {
            Mobile mobile = GetMobile(p.ReadUInt());
            if (!mobile.IsValid)
                throw new Exception("OnMobileStatus - !mobile.IsValid");//does this happen?

            lock (mobile.SyncRoot)
            {
                mobile.Name = p.ReadStringAscii(30);
                mobile.Hits = p.ReadUShort();
                mobile.HitsMax = p.ReadUShort();
                mobile.Renamable = p.ReadBool();

                byte type = p.ReadByte();
                if (type > 0)
                {
                    Player.Female = p.ReadBool();
                    Player.Strength = p.ReadUShort();
                    Player.Dexterity = p.ReadUShort();
                    Player.Intelligence = p.ReadUShort();
                    Player.Stamina = p.ReadUShort();
                    Player.StaminaMax = p.ReadUShort();
                    Player.Mana = p.ReadUShort();
                    Player.ManaMax = p.ReadUShort();
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
            }
            mobile.ProcessDelta();
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

            lock (mobile.SyncRoot)
                mobile.Flags = p.ReadBool() ? mobile.Flags | flag : mobile.Flags & ~flag;
            mobile.ProcessDelta();
        }
    }
}