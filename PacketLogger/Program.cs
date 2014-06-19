using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace PacketLogger
{
    class Program
    {
        static void Main()
        {
            UOInterface.Start("C:\\UO\\Test\\client.exe", null, "PacketLogger.Program", "EntryPoint", null);
        }

        private static UOInterface.CallBacks c;
        public static int EntryPoint(String args)
        {
            c = new UOInterface.CallBacks
            {
                OnMessage = onMessage,
                OnKeyDown = onKeyDown,
                OnSend = onSend,
                OnRecv = onRecv
            };
            UOInterface.InstallHooks(c, true);
            UOInterface.SetConnectionInfo(0x0100007F, 2593);//127.0.0.1, 2593
            new Thread(() => AllocConsole()).Start();
            return 0;
        }

        private static void onMessage(UOInterface.Message msg, uint param)
        {
            Console.Write("Message: {0}", msg);
            switch (msg)
            {
                case UOInterface.Message.Focus:
                case UOInterface.Message.Visibility:
                    Console.Write(" - {0}", param != 0);
                    break;
                case UOInterface.Message.WindowCreated:
                    Console.Write(" - 0x{0:X8}", param);
                    break;
            }
            Console.WriteLine();
        }

        private static bool onKeyDown(uint virtualKey)
        {
            Console.WriteLine("Keys: {0}", (Keys)virtualKey);
            return false;//== do not filter
        }

        private static bool onSend(byte[] buffer, int len)
        {
            WritePacket(buffer, "Client -> Server");
            if (buffer[0] == 0xAD)
            {//duplicate sent messages - just for fun (and for testing if it really works...)
                byte[] asdf = new byte[len];
                buffer.CopyTo(asdf, 0);
                UOInterface.SendToServer(asdf);
            }
            return false;
        }

        private static bool onRecv(byte[] buffer, int len)
        {
            WritePacket(buffer, "Server -> Client");
            if (buffer[0] == 0xAE)
            {//duplicate recieved messages - just for fun (and for testing if it really works...)
                byte[] asdf = new byte[len];
                buffer.CopyTo(asdf, 0);
                UOInterface.SendToClient(asdf);
            }
            return false;
        }

        private static void WritePacket(byte[] buffer, string direction)
        {
            Console.WriteLine("{0}\t {1:X2} - {2} bytes", direction, buffer[0], buffer.Length);
            Console.WriteLine(" 0  1  2  3  4  5  6  7   8  9  A  B  C  D  E  F");
            Console.WriteLine("-- -- -- -- -- -- -- --  -- -- -- -- -- -- -- --");

            for (int i = 0; i < buffer.Length; i++)
            {
                if (i % 16 == 0 && i != 0)
                    Console.WriteLine();
                if (i % 8 == 0 && i % 16 != 0)
                    Console.Write(" ");
                Console.Write(buffer[i].ToString("X2"));
                Console.Write(" ");
            }

            Console.WriteLine();
            Console.WriteLine();
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool AllocConsole();
    }
}