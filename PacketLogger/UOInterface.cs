using System;
using System.Runtime.InteropServices;

namespace PacketLogger
{
    public static class UOInterface
    {
        public enum UOMessage
        {
            Init = 0x0400, PacketLengths, Focus, Visibility, Disconnect, ExitProcess,
            KeyDown, PacketToServer, PacketToClient, ConnectionInfo, Patch
        };

        public const string MemoryNameIn = "UOInterface_In_";
        public const string MemoryNameOut = "UOInterface_Out_";

        [DllImport("UOInterface.dll", CharSet = CharSet.Unicode)]
        public static extern int Start(string client, IntPtr hwnd);

        [DllImport("UOInterface.dll", CharSet = CharSet.Unicode)]
        public static extern void Inject(int pid, IntPtr hwnd);

        [DllImport("user32.dll")]
        public static extern IntPtr SendMessage(IntPtr hWnd, UOMessage msg, IntPtr wParam = default(IntPtr), IntPtr lParam = default(IntPtr));
    }
}