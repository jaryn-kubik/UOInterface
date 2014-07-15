using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using UOInterface.Network;

namespace UOInterface
{
    public static class Client
    {
        public static bool PatchEncryption { get; set; }
        public static bool PatchMulti { get; set; }
        public static uint ServerIP { get; set; }
        public static ushort ServerPort { get; set; }
        public static Version Version { get; private set; }

        public static event EventHandler Connected, Disconnecting, Closing;
        public static event EventHandler<bool> FocusChanged, VisibilityChanged;
        public static event EventHandler<UOKeyEventArgs> KeyDown;
        public static event EventHandler<Packet> PacketToClient, PacketToServer;

        private static IntPtr bufferOut;
        private static unsafe byte* bufferIn;
        private static unsafe short* packetTable;
        private static readonly object packetSync = new object();

        public static unsafe void Start(string client)
        {
            int pid = Start(client, onMessage);
            FileVersionInfo v = Process.GetProcessById(pid).MainModule.FileVersionInfo;
            Version = new Version(v.FileMajorPart, v.FileMinorPart, v.FileBuildPart, v.FilePrivatePart);
            bufferIn = GetInBuffer();
            bufferOut = new IntPtr(GetOutBuffer());
            packetTable = GetPacketTable();
        }

        public static unsafe short GetPacketLength(byte id)
        {
            short len = packetTable[id];
            if (len == 0)
                throw new ArgumentException("Packet doesn't exist.", "id");
            return packetTable[id];
        }

        public static void Pathfind(ushort x, ushort y, ushort z)
        {
            SendUOMessage(UOMessage.Pathfinding, (x << 16 | y), z);
        }

        public static void SendToClient(byte[] buffer)
        {
            lock (packetSync)
            {
                Marshal.Copy(buffer, 0, bufferOut, buffer.Length);
                SendUOMessage(UOMessage.PacketToClient);
            }
        }

        public static void SendToServer(byte[] buffer)
        {
            lock (packetSync)
            {
                Marshal.Copy(buffer, 0, bufferOut, buffer.Length);
                SendUOMessage(UOMessage.PacketToServer);
            }
        }

        private static readonly OnUOMessage onMessage = OnMessage;
        private static unsafe uint OnMessage(UOMessage msg, int wParam, int lParam)
        {
            switch (msg)
            {
                case UOMessage.Init:
                    SendUOMessage(UOMessage.ConnectionInfo, (int)ServerIP, ServerPort);
                    SendUOMessage(UOMessage.Patch, PatchEncryption ? 1 : 0, PatchMulti ? 1 : 0);
                    break;

                case UOMessage.Connected:
                    Connected.Raise();
                    break;

                case UOMessage.Disconnecting:
                    Disconnecting.Raise();
                    break;

                case UOMessage.Closing:
                    Closing.Raise();
                    break;

                case UOMessage.Focus:
                    FocusChanged.Raise(wParam != 0);
                    break;

                case UOMessage.Visibility:
                    VisibilityChanged.Raise(wParam != 0);
                    break;

                case UOMessage.KeyDown:
                    UOKeyEventArgs keyArgs = new UOKeyEventArgs(wParam, lParam);
                    KeyDown.Raise(keyArgs);
                    if (keyArgs.Filter)
                        return 1;
                    break;

                case UOMessage.PacketToClient:
                    Packet toClient = new Packet(bufferIn, wParam);
                    PacketToClient.Raise(toClient);
                    if (toClient.Filter)
                        return 1;
                    if (toClient.Changed)
                        return 2;
                    break;

                case UOMessage.PacketToServer:
                    Packet toServer = new Packet(bufferIn, wParam);
                    PacketToServer.Raise(toServer);
                    if (toServer.Filter)
                        return 1;
                    if (toServer.Changed)
                        return 2;
                    break;
            }
            return 0;
        }

        #region UOInterface
        [DllImport("UOInterface.dll", CharSet = CharSet.Unicode)]
        private static extern int Start(string client, OnUOMessage onMessage);

        [DllImport("UOInterface.dll")]
        private static extern unsafe byte* GetInBuffer();

        [DllImport("UOInterface.dll")]
        private static extern unsafe byte* GetOutBuffer();

        [DllImport("UOInterface.dll")]
        private static extern unsafe short* GetPacketTable();

        [DllImport("UOInterface.dll")]
        private static extern void SendUOMessage(UOMessage msg, int wParam = 0, int lParam = 0);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate uint OnUOMessage(UOMessage msg, int wParam, int lParam);
        private enum UOMessage
        {
            Init = 0x0400, Connected, Disconnecting, Closing, Focus, Visibility,
            KeyDown, PacketToClient, PacketToServer,
            ConnectionInfo, Pathfinding, Patch
        }
        #endregion
    }

    public class UOKeyEventArgs : EventArgs
    {
        internal UOKeyEventArgs(int virtualCode, int mods)
        {
            VirtualCode = virtualCode;
            Modifiers = mods;
        }

        public int VirtualCode { get; private set; }
        public int Modifiers { get; private set; }
        public bool Alt { get { return (Modifiers & 1) != 0; } }
        public bool Control { get { return (Modifiers & 2) != 0; } }
        public bool Shift { get { return (Modifiers & 4) != 0; } }
        public bool Filter { get; set; }
    }
}