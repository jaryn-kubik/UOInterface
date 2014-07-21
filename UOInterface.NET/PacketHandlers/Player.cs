using System;
using UOInterface.Network;

namespace UOInterface
{
    public static partial class World
    {
        private static void OnLoginConfirm(Packet p)//0x1B
        {
            Clear();
            mobilesToAdd.Add(Player = new PlayerMobile(p.ReadUInt()));
            p.Skip(4);//unknown
            Player.Graphic = p.ReadUShort();
            Player.Position = new Position(p.ReadUShort(), p.ReadUShort(), (sbyte)p.ReadUShort());
            Player.Direction = (Direction)p.ReadByte();
            //p.Skip(9);//unknown
            //p.ReadUShort();//map width
            //p.ReadUShort();//map height
            toProcess.Enqueue(Player);
            ProcessDelta();
        }

        private static void OnPlayerUpdate(Packet p)//0x20
        {
            if (p.ReadUInt() != Player)
                throw new Exception("OnMobileStatus");//does this happen?
            movementQueue.Clear();

            Player.Graphic = (ushort)(p.ReadUShort() + p.ReadSByte());
            Player.Hue = p.ReadUShort();
            Player.Flags = (UOFlags)p.ReadByte();

            ushort x = p.ReadUShort();
            ushort y = p.ReadUShort();
            p.Skip(2);//unknown
            Player.Direction = (Direction)p.ReadByte();
            Player.Position = new Position(x, y, p.ReadSByte());

            OnPlayerMoved();
            toProcess.Enqueue(Player);
            ProcessDelta();
        }

        private static void OnWarMode(Packet p) //0x72
        {
            Player.WarMode = p.ReadBool();
            toProcess.Enqueue(Player);
            ProcessDelta();
        }

        private static void OnSkillUpdate(Packet p)//0x3A
        {
            ushort id;
            switch (p.ReadByte())
            {
                case 0:
                    while ((id = p.ReadUShort()) > 0)
                        Player.UpdateSkill(id - 1, p.ReadUShort(), p.ReadUShort(), (SkillLock)p.ReadByte(), 100);
                    break;

                case 2:
                    while ((id = p.ReadUShort()) > 0)
                        Player.UpdateSkill(id - 1, p.ReadUShort(), p.ReadUShort(), (SkillLock)p.ReadByte(), p.ReadUShort());
                    break;

                case 0xDF:
                    id = p.ReadUShort();
                    Player.UpdateSkill(id, p.ReadUShort(), p.ReadUShort(), (SkillLock)p.ReadByte(), p.ReadUShort());
                    break;

                case 0xFF:
                    id = p.ReadUShort();
                    Player.UpdateSkill(id, p.ReadUShort(), p.ReadUShort(), (SkillLock)p.ReadByte(), 100);
                    break;
            }
            toProcess.Enqueue(Player);
            ProcessDelta();
        }

        private static void OnChangeSkillLock(Packet p)//0x3A
        {
            Player.UpdateSkillLock(p.ReadUShort(), (SkillLock)p.ReadByte());
            toProcess.Enqueue(Player);
            ProcessDelta();
        }
    }
}