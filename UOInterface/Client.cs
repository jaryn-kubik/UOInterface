using System;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using UOInterface.Network;

namespace UOInterface
{
    public static class Client
    {
        public static bool PatchEncryption { get; set; }
        public static uint ServerIP { get; set; }
        public static ushort ServerPort { get; set; }
        public static Version Version { get; private set; }
        public static ushort Width { get; set; }
        public static ushort Height { get; set; }
        public static bool Ready { get; private set; }

        public static event EventHandler Connected, Disconnecting, Closing;
        public static event EventHandler<bool> FocusChanged, VisibilityChanged;
        public static event EventHandler<UOKeyEventArgs> KeyDown;
        public static event EventHandler<Packet> PacketToClient, PacketToServer;
        public static event UnhandledExceptionEventHandler UnhandledException;

        private static unsafe byte* bufferIn, bufferOut;
        private static unsafe short* packetTable;
        private static readonly object packetSync = new object();

        public static unsafe void Start(string client)
        {
            int pid = Start(client, onMessage);
            FileVersionInfo v = Process.GetProcessById(pid).MainModule.FileVersionInfo;
            Version = new Version(v.FileMajorPart, v.FileMinorPart, v.FileBuildPart, v.FilePrivatePart);
            bufferIn = GetInBuffer();
            bufferOut = GetOutBuffer();
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

        public static unsafe void SendToClient(byte[] buffer, int len = 0)
        {
            lock (packetSync)
            {
                fixed (byte* b = buffer)
                    memcpy(bufferOut, b, len > 0 ? len : buffer.Length);
                SendUOMessage(UOMessage.PacketToClient);
            }
        }

        public static unsafe void SendToServer(byte[] buffer, int len = 0)
        {
            lock (packetSync)
            {
                fixed (byte* b = buffer)
                    memcpy(bufferOut, b, len > 0 ? len : buffer.Length);
                SendUOMessage(UOMessage.PacketToServer);
            }
        }

        private static readonly OnUOMessage onMessage = OnMessage;
        private static int OnMessage(UOMessage msg, int wParam, int lParam)
        {
            try { return HandleMessage(msg, wParam, lParam); }
            catch (Exception ex)
            {
                OnException(ex);
                return 0;
            }
        }

        private static unsafe int HandleMessage(UOMessage msg, int wParam, int lParam)
        {
            switch (msg)
            {
                case UOMessage.Ready:
                    Ready = true;
                    OnInit();
                    SendUOMessage(UOMessage.ConnectionInfo, (int)ServerIP, ServerPort);
                    SendUOMessage(UOMessage.GameSize, Width, Height);
                    return PatchEncryption ? 1 : 0;

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

        private static void OnInit()
        {
            foreach (MethodInfo m in typeof(Client).Assembly.GetTypes()
               .SelectMany(t => t.GetMethods(BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Static))
               .Where(m => m.IsDefined(typeof(OnInitAttribute))))
                m.Invoke(null, null);
        }

        internal static void OnException(Exception ex)
        {
            var handler = UnhandledException;
            if (handler == null)
                throw ex;
            handler(null, new UnhandledExceptionEventArgs(ex, false));
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

        [DllImport("msvcrt.dll")]
        private static unsafe extern void memcpy(void* to, void* from, int len);

        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        private delegate int OnUOMessage(UOMessage msg, int wParam, int lParam);
        private enum UOMessage
        {
            Ready = 0x0400, Connected, Disconnecting, Closing, Focus, Visibility,
            KeyDown, PacketToClient, PacketToServer,
            ConnectionInfo, Pathfinding, GameSize
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

    [AttributeUsage(AttributeTargets.Method)]
    internal sealed class OnInitAttribute : Attribute { }
}