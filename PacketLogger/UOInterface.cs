using System;
using System.Runtime.InteropServices;

namespace PacketLogger
{
    public static class UOInterface
    {
        public enum UOMessage
        {
            ExitProcess = 0x0400, Disconnect, WindowCreated, Focus, Visibility,
            KeyDown, ConnectionInfo, PacketLengths, PacketToServer, PacketToClient
        };

        public const string MemoryNameIn = "UOInterface_In_";
        public const string MemoryNameOut = "UOInterface_Out_";

        [DllImport("UOInterface.dll", CharSet = CharSet.Unicode)]
        public static extern int Start(string client, IntPtr hwnd);

        [DllImport("UOInterface.dll", CharSet = CharSet.Unicode)]
        public static extern void Inject(int pid, IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, UOMessage msg, IntPtr wParam, IntPtr lParam);
    }
}