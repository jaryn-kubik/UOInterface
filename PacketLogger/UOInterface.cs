using System;
using System.Runtime.InteropServices;

namespace PacketLogger
{
    public enum UOMessage
    {
        Init = 0x0400, Focus, Visibility, Disconnect, ExitProcess,
        KeyDown, PacketToClient, PacketToServer,
        ConnectionInfo, Pathfinding, Patch
    }

    public static class UOInterface
    {
        [DllImport("UOInterface.dll", CharSet = CharSet.Unicode)]
        public static extern void Start(string client, IntPtr hwnd);

        [DllImport("UOInterface.dll", CharSet = CharSet.Unicode)]
        public static extern void Inject(int pid, IntPtr hwnd);

        [DllImport("UOInterface.dll")]
        public static extern IntPtr GetInBuffer();

        [DllImport("UOInterface.dll")]
        public static extern IntPtr GetOutBuffer();

        [DllImport("UOInterface.dll")]
        public static extern IntPtr GetPacketTable();

        [DllImport("UOInterface.dll")]
        public static extern void SendIPCMessage(UOMessage msg, uint wParam = 0, uint lParam = 0);
    }
}