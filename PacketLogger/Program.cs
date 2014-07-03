using System;
using System.IO.MemoryMappedFiles;
using System.Windows.Forms;

namespace PacketLogger
{
    class Program
    {
        private static IntPtr hwnd;
        private static MemoryMappedViewAccessor mIn, mOut;
        private static ushort[] packetLengths;

        static void Main()
        {
            uownd wnd = new uownd();
            int pid = UOInterface.Start("C:\\UO\\Test\\client.exe", wnd.Handle);

            mIn = MemoryMappedFile.OpenExisting(UOInterface.MemoryNameIn + pid).CreateViewAccessor();
            mOut = MemoryMappedFile.OpenExisting(UOInterface.MemoryNameOut + pid).CreateViewAccessor();
            Application.Run();
        }

        private class uownd : NativeWindow
        {
            public uownd() { CreateHandle(new CreateParams()); }
            protected override void WndProc(ref Message m)
            {
                if (Enum.IsDefined(typeof(UOMessage), m.Msg))
                {
                    bool result = onMessage((UOMessage)m.Msg, m.WParam);
                    m.Result = new IntPtr(Convert.ToInt32(result));
                }
                else
                    base.WndProc(ref m);
            }
        }

        private static byte[] readPacket(IntPtr wParam)
        {
            int len = wParam.ToInt32();
            byte[] data = new byte[len];
            mOut.ReadArray(0, data, 0, len);
            return data;
        }

        private static bool onMessage(UOMessage msg, IntPtr wParam)
        {
            Console.Write(msg);
            switch (msg)
            {
                case UOMessage.Init:
                    hwnd = wParam;
                    IntPtr ip = new IntPtr(0x0100007F);//127.0.0.1
                    IntPtr port = new IntPtr(2593);
                    UOInterface.SendMessage(hwnd, UOMessage.ConnectionInfo, ip, port);
                    UOInterface.SendMessage(hwnd, UOMessage.Patch, new IntPtr(1), new IntPtr(1));
                    Console.Write(" - 0x{0:X8}", (uint)wParam.ToInt32());
                    break;

                case UOMessage.PacketLengths:
                    int len = wParam.ToInt32() / sizeof(ushort);
                    packetLengths = new ushort[len];
                    mOut.ReadArray(0, packetLengths, 0, len);
                    Console.Write(" - {0} packets", len);
                    break;

                case UOMessage.Focus:
                case UOMessage.Visibility:
                    Console.Write(" - {0}", wParam.ToInt32() != 0);
                    break;

                case UOMessage.KeyDown:
                    Console.Write(" - {0}", (Keys)(uint)wParam.ToInt32());
                    break;

                case UOMessage.PacketToClient:
                    return onRecv(readPacket(wParam));

                case UOMessage.PacketToServer:
                    return onSend(readPacket(wParam));
            }
            Console.WriteLine();
            return false;
        }

        private static bool onSend(byte[] buffer)
        {
            WritePacket(buffer);
            if (buffer[0] == 0xAD)
            {//duplicate sent chat messages - just for fun (and for testing if it really works...)
                mIn.WriteArray(0, buffer, 0, buffer.Length);
                UOInterface.SendMessage(hwnd, UOMessage.PacketToServer);
            }
            return false;
        }

        private static bool onRecv(byte[] buffer)
        {
            WritePacket(buffer);
            if (buffer[0] == 0xAE)
            {//duplicate recieved chat messages - just for fun (and for testing if it really works...)
                mIn.WriteArray(0, buffer, 0, buffer.Length);
                UOInterface.SendMessage(hwnd, UOMessage.PacketToClient);
            }
            return false;
        }

        private static void WritePacket(byte[] buffer)
        {
            Console.WriteLine("\t\t {0:X2} - {1} bytes", buffer[0], buffer.Length);
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
    }
}