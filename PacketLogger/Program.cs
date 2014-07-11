using System;
using System.Runtime.InteropServices;
using UOInterface;

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

            Client.Start("C:\\UO\\Test\\client.exe");
        }

        private static void Client_PacketToClient(object sender, PacketEventArgs e)
        {
            Console.Write("PacketToClient");
            WritePacket(e.Data, e.Length);
            if (Marshal.ReadByte(e.Data) == 0xAE)
            {//duplicate recieved chat messages - just for fun (and for testing if it really works...)
                byte[] data = new byte[e.Length];
                Marshal.Copy(e.Data, data, 0, e.Length);
                Client.SendToClient(data);
            }
        }

        private static void Client_PacketToServer(object sender, PacketEventArgs e)
        {
            Console.Write("PacketToServer");
            WritePacket(e.Data, e.Length);
            if (Marshal.ReadByte(e.Data) == 0xAD)
            {//duplicate sent chat messages - just for fun (and for testing if it really works...)
                byte[] data = new byte[e.Length];
                Marshal.Copy(e.Data, data, 0, e.Length);
                Client.SendToServer(data);
            }
        }

        private static void WritePacket(IntPtr data, int len)
        {
            Console.WriteLine("\t\t {0:X2} - {1} bytes", Marshal.ReadByte(data), len);
            Console.WriteLine(" 0  1  2  3  4  5  6  7   8  9  A  B  C  D  E  F");
            Console.WriteLine("-- -- -- -- -- -- -- --  -- -- -- -- -- -- -- --");

            for (int i = 0; i < len; i++)
            {
                if (i % 16 == 0 && i != 0)
                    Console.WriteLine();
                if (i % 8 == 0 && i % 16 != 0)
                    Console.Write(" ");
                Console.Write(Marshal.ReadByte(data, i).ToString("X2"));
                Console.Write(" ");
            }

            Console.WriteLine();
            Console.WriteLine();
        }
    }
}
