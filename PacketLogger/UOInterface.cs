using System.Runtime.InteropServices;

namespace PacketLogger
{
    public static class UOInterface
    {
        [DllImport("UOInterface.dll", CharSet = CharSet.Unicode)]
        public static extern void Start(string client, string assembly, string typeName, string methodName, string args);

        [DllImport("UOInterface.dll")]
        public static extern void InstallHooks(CallBacks callBacks, bool patchEncryption);

        [DllImport("UOInterface.dll")]
        public static extern void SetConnectionInfo(uint address, ushort port);

        [DllImport("UOInterface.dll")]
        public static extern uint GetPacketLength(byte packetId);

        [DllImport("UOInterface.dll")]
        public static extern void SendToClient([MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1)]byte[] buffer);

        [DllImport("UOInterface.dll")]
        public static extern void SendToServer([MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1)]byte[] buffer);

        [StructLayout(LayoutKind.Sequential)]
        public struct CallBacks
        {//you have to keep instance of this structure alive!!!
            public dMessage OnMessage;
            public dKeyDown OnKeyDown;
            public dPacket OnRecv;
            public dPacket OnSend;
        }

        public enum Message : byte { ExitProcess, Disconnect, WindowCreated, Focus, Visibility};
        public delegate void dMessage(Message msg, uint param);
        public delegate bool dKeyDown(uint virtualKey);
        public delegate bool dPacket([In, Out][MarshalAs(UnmanagedType.LPArray, ArraySubType = UnmanagedType.U1, SizeParamIndex = 1)]byte[] buffer, int len);
    }
}