using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace PacketLogger
{
    class Program
    {
        private static IntPtr bufferIn, bufferOut;
        private static readonly short[] packetTable = new short[0x100];

        static void Main()
        {
            uownd wnd = new uownd();
            UOInterface.Start("C:\\UO\\Test\\client.exe", wnd.Handle);
            bufferIn = UOInterface.GetInBuffer();
            bufferOut = UOInterface.GetOutBuffer();
            Marshal.Copy(UOInterface.GetPacketTable(), packetTable, 0, packetTable.Length);
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
            Marshal.Copy(bufferIn, data, 0, len);
            return data;
        }

        private static bool onMessage(UOMessage msg, IntPtr wParam)
        {
            Console.Write(msg);
            switch (msg)
            {
                case UOMessage.Init:
                    UOInterface.SendIPCMessage(UOMessage.ConnectionInfo, 0x0100007F, 2593);//127.0.0.1, 2593
                    UOInterface.SendIPCMessage(UOMessage.Patch, 1, 1);
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
                Marshal.Copy(buffer, 0, bufferOut, buffer.Length);
                UOInterface.SendIPCMessage(UOMessage.PacketToServer);
            }
            return false;
        }

        private static bool onRecv(byte[] buffer)
        {
            WritePacket(buffer);
            if (buffer[0] == 0xAE)
            {//duplicate recieved chat messages - just for fun (and for testing if it really works...)
                Marshal.Copy(buffer, 0, bufferOut, buffer.Length);
                UOInterface.SendIPCMessage(UOMessage.PacketToClient);
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