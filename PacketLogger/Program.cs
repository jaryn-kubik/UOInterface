using System;
using UOInterface;
using UOInterface.Network;

namespace PacketLogger
{
    class Program
    {
        static void Main()
        {
            Client.ServerIP = 0x0100007F;//127.0.0.1
            Client.ServerPort = 2593;
            Client.PatchEncryption = Client.PatchMulti = true;

            Client.Connected += (s, e) => Console.WriteLine("Connected");
            Client.Disconnecting += (s, e) => Console.WriteLine("Disconnecting");
            Client.Closing += (s, e) => Console.WriteLine("Closing");

            Client.FocusChanged += (s, e) => Console.WriteLine("FocusChanged - " + e);
            Client.VisibilityChanged += (s, e) => Console.WriteLine("VisibilityChanged - " + e);

            Client.KeyDown += (s, e) => Console.WriteLine("KeyDown - " + e.KeyData);
            Client.PacketToClient += Client_PacketToClient;
            Client.PacketToServer += Client_PacketToServer;

            Client.Start("C:\\UO\\Auberon\\client.exe");
            Console.WriteLine("Client version: " + Client.Version);
        }

        private static void Client_PacketToClient(object sender, Packet p)
        {
            Console.Write("PacketToClient");
            WritePacket(p);
            if (p.ID == 0xAE)
                //duplicate recieved chat messages - just for fun (and for testing if it really works...)
                Client.SendToClient(p.ToArray());
        }

        private static void Client_PacketToServer(object sender, Packet p)
        {
            Console.Write("PacketToServer");
            WritePacket(p);
            if (p.ID == 0xAD)
                //duplicate sent chat messages - just for fun (and for testing if it really works...)
                Client.SendToServer(p.ToArray());
        }

        private static void WritePacket(Packet packet)
        {
            Console.WriteLine("\t\t {0:X2} - {1} bytes", packet.ID, packet.Length);
            Console.WriteLine(" 0  1  2  3  4  5  6  7   8  9  A  B  C  D  E  F");
            Console.WriteLine("-- -- -- -- -- -- -- --  -- -- -- -- -- -- -- --");

            packet.Seek(0);
            for (int i = 0; i < packet.Length; i++)
            {
                if (i % 16 == 0 && i != 0)
                    Console.WriteLine();
                if (i % 8 == 0 && i % 16 != 0)
                    Console.Write(" ");
                Console.Write(packet.ReadByte().ToString("X2"));
                Console.Write(" ");
            }

            Console.WriteLine();
            Console.WriteLine();
        }
    }
}
