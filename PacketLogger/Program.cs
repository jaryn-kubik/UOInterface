using System;
using System.Runtime.InteropServices;
using System.Threading;

namespace PacketLogger
{
    class Program
    {
        static void Main()
        {
            UOInterface.Start("C:\\UO\\Auberon\\client.exe", null, "PacketLogger.Program", "EntryPoint", null);
        }

        private static UOInterface.CallBacks c;
        public static int EntryPoint(String args)
        {
            UOInterface.PatchEncryption();
            c = new UOInterface.CallBacks
            {
                OnDisconnect = () => { },
                OnExitProcess = () => { },

                OnWindowCreated = a => { },
                OnFocus = a => { },
                OnVisibility = a => { },
                OnKeyDown = (a, b) => false,

                OnSend = onSend,
                OnRecv = onRecv
            };
            UOInterface.SetCallbacks(c);
            new Thread(() => AllocConsole()).Start();
            return 0;
        }

        private static bool onSend(byte[] buffer, int len)
        {
            WritePacket(buffer, "Client -> Server");
            return false;
        }

        private static bool onRecv(byte[] buffer, int len)
        {
            WritePacket(buffer, "Server -> Client");
            return false;
        }

        private static void WritePacket(byte[] buffer, string direction)
        {
            Console.WriteLine("{0}\t{1:X2} - {2} bytes", direction, buffer[0], buffer.Length);
            Console.WriteLine(" 0  1  2  3  4  5  6  7   8  9  A  B  C  D  E  F");
            Console.WriteLine("-- -- -- -- -- -- -- --  -- -- -- -- -- -- -- --");

            for (int i = 0; i < buffer.Length; i++)
            {
                if (i%16 == 0 && i != 0)
                    Console.WriteLine();
                Console.Write(buffer[i].ToString("X2"));
                Console.Write(" ");
                if (i%8 == 0 && i%16 != 0)
                    Console.Write(" ");
            }

            Console.WriteLine();
            Console.WriteLine();
        }

        [DllImport("kernel32.dll", SetLastError = true)]
        public static extern bool AllocConsole();
    }
}
