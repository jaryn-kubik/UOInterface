using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace UOInterface
{
    public static class Client
    {
        private sealed class ClientWindow : NativeWindow
        {
            public ClientWindow() { CreateHandle(new CreateParams()); }
            protected override void WndProc(ref Message m)
            {
                if (m.Msg >= (int)UOMessage.First && m.Msg <= (int)UOMessage.Last)
                    m.Result = OnMessage((UOMessage)m.Msg, m.WParam.ToInt32());
                else
                    base.WndProc(ref m);
            }
        }

        private static readonly IntPtr ptrOne = new IntPtr(1), ptrTwo = new IntPtr(2);
        public static bool PatchEncryption { get; set; }
        public static bool PatchMulti { get; set; }
        public static uint ServerIP { get; set; }
        public static ushort ServerPort { get; set; }

        public static event EventHandler Connected, Disconnecting, Closing;
        public static event EventHandler<bool> FocusChanged, VisibilityChanged;
        public static event EventHandler<KeyEventArgs> KeyDown;
        public static event EventHandler<PacketEventArgs> PacketToClient, PacketToServer;

        private static IntPtr bufferOut;
        private static unsafe byte* bufferIn;
        private static unsafe short* packetTable;
        private static readonly object packetSync = new object();

        public static unsafe void Start(string client)
        {
            IntPtr handle = IntPtr.Zero;
            ManualResetEvent ready = new ManualResetEvent(false);
            new Thread(() =>
            {
                handle = new ClientWindow().Handle;
                ready.Set();
                Application.Run();
            }).Start();
            ready.WaitOne();

            Start(client, handle);
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
            SendIPCMessage(UOMessage.Pathfinding, (uint)(x << 16 | y), z);
        }

        public static void SendToClient(byte[] buffer)
        {
            lock (packetSync)
            {
                Marshal.Copy(buffer, 0, bufferOut, buffer.Length);
                SendIPCMessage(UOMessage.PacketToClient);
            }
        }

        public static void SendToServer(byte[] buffer)
        {
            lock (packetSync)
            {
                Marshal.Copy(buffer, 0, bufferOut, buffer.Length);
                SendIPCMessage(UOMessage.PacketToServer);
            }
        }

        private static unsafe IntPtr OnMessage(UOMessage msg, int wParam)
        {
            switch (msg)
            {
                case UOMessage.Init:
                    SendIPCMessage(UOMessage.ConnectionInfo, ServerIP, ServerPort);
                    SendIPCMessage(UOMessage.Patch, Convert.ToUInt32(PatchEncryption), Convert.ToUInt32(PatchMulti));
                    break;

                case UOMessage.Connected:
                    EventHandler connected = Connected;
                    if (connected != null)
                        connected(null, EventArgs.Empty);
                    break;

                case UOMessage.Disconnecting:
                    EventHandler diconnecting = Disconnecting;
                    if (diconnecting != null)
                        diconnecting(null, EventArgs.Empty);
                    break;

                case UOMessage.Closing:
                    EventHandler closing = Closing;
                    if (closing != null)
                        closing(null, EventArgs.Empty);
                    Application.ExitThread();
                    break;

                case UOMessage.Focus:
                    EventHandler<bool> focusChanged = FocusChanged;
                    if (focusChanged != null)
                        focusChanged(null, wParam != 0);
                    break;

                case UOMessage.Visibility:
                    EventHandler<bool> visibilityChanged = VisibilityChanged;
                    if (visibilityChanged != null)
                        visibilityChanged(null, wParam != 0);
                    break;

                case UOMessage.KeyDown:
                    EventHandler<KeyEventArgs> keyDown = KeyDown;
                    KeyEventArgs keyArgs = new KeyEventArgs((Keys)wParam);
                    if (keyDown != null)
                        keyDown(null, keyArgs);
                    if (keyArgs.Handled)
                        return ptrOne;
                    break;

                case UOMessage.PacketToClient:
                    EventHandler<PacketEventArgs> toClient = PacketToClient;
                    PacketEventArgs toClientArgs = new PacketEventArgs(bufferIn, wParam);
                    if (toClient != null)
                        toClient(null, toClientArgs);
                    if (toClientArgs.Filter)
                        return ptrOne;
                    if (toClientArgs.Packet.Changed)
                        return ptrTwo;
                    break;

                case UOMessage.PacketToServer:
                    EventHandler<PacketEventArgs> toServer = PacketToServer;
                    PacketEventArgs toServerArgs = new PacketEventArgs(bufferIn, wParam);
                    if (toServer != null)
                        toServer(null, toServerArgs);
                    if (toServerArgs.Filter)
                        return ptrOne;
                    if (toServerArgs.Packet.Changed)
                        return ptrTwo;
                    break;
            }
            return IntPtr.Zero;
        }

        #region UOInterface
        [DllImport("UOInterface.dll", CharSet = CharSet.Unicode)]
        private static extern void Start(string client, IntPtr hwnd);

        [DllImport("UOInterface.dll")]
        private static extern unsafe byte* GetInBuffer();

        [DllImport("UOInterface.dll")]
        private static extern unsafe byte* GetOutBuffer();

        [DllImport("UOInterface.dll")]
        private static extern unsafe short* GetPacketTable();

        [DllImport("UOInterface.dll")]
        private static extern void SendIPCMessage(UOMessage msg, uint wParam = 0, uint lParam = 0);

        private enum UOMessage
        {
            First = 0x0400,
            Init = First, Connected, Disconnecting, Closing, Focus, Visibility,
            KeyDown, PacketToClient, PacketToServer,
            ConnectionInfo, Pathfinding, Patch,
            Last = Patch
        }
        #endregion
    }

    public class PacketEventArgs : EventArgs
    {
        public unsafe PacketEventArgs(byte* data, int len)
        { Packet = new Packet(data, len); }

        public Packet Packet { get; private set; }
        public bool Filter { get; set; }
    }
}