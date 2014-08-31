using System;
using System.Linq;
using System.Reflection;
using UOInterface.Network;

namespace UOInterface
{
    public static class Client
    {
        public static bool PatchEncryption { get; set; }
        public static uint ServerIP { get; set; }
        public static ushort ServerPort { get; set; }
        public static ushort Width { get; set; }
        public static ushort Height { get; set; }
        public static bool Ready { get; private set; }
        public static Version Version { get { return hooks.Version; } }

        public static event EventHandler Connected, Disconnecting, Closing;
        public static event EventHandler<bool> FocusChanged, VisibilityChanged;
        public static event EventHandler<UOKeyEventArgs> KeyDown;
        public static event EventHandler<Packet> PacketToClient, PacketToServer;
        public static event UnhandledExceptionEventHandler UnhandledException;

        private static UOHooks hooks;
        public static unsafe void Start(string client) { hooks = UOHooks.Start(client, OnMessage); }

        public static short GetPacketLength(byte id)
        {
            uint len = hooks.PacketTable[id];
            if (len == 0)
                throw new ArgumentException("Packet doesn't exist.", "id");
            return (short)len;
        }

        public static void Pathfind(ushort x, ushort y, ushort z)
        { hooks.Send(UOMessage.Pathfinding, x, y, z); }

        public static void SendToClient(byte[] buffer, int len = 0)
        { hooks.SendData(UOMessage.PacketToClient, buffer, len > 0 ? len : buffer.Length); }

        public static void SendToServer(byte[] buffer, int len = 0)
        { hooks.SendData(UOMessage.PacketToServer, buffer, len > 0 ? len : buffer.Length); }

        private static unsafe int OnMessage(UOMessage msg, int arg1, int arg2, int arg3, byte* data)
        {
            try
            {
                switch (msg)
                {
                    case UOMessage.Ready:
                        Ready = true;
                        hooks.Send(UOMessage.ConnectionInfo, (int)ServerIP, ServerPort, PatchEncryption ? 1 : 0);
                        hooks.Send(UOMessage.GameSize, Width, Height);
                        OnInit();
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
                        FocusChanged.Raise(arg1 != 0);
                        break;

                    case UOMessage.Visibility:
                        VisibilityChanged.Raise(arg1 != 0);
                        break;

                    case UOMessage.KeyDown:
                        UOKeyEventArgs keyArgs = new UOKeyEventArgs(arg1, arg2);
                        KeyDown.Raise(keyArgs);
                        if (keyArgs.Filter)
                            return 1;
                        break;

                    case UOMessage.PacketToClient:
                        Packet toClient = new Packet(data, arg1);
                        PacketToClient.Raise(toClient);
                        if (toClient.Filter)
                            return 1;
                        if (toClient.Changed)
                            return 2;
                        break;

                    case UOMessage.PacketToServer:
                        Packet toServer = new Packet(data, arg1);
                        PacketToServer.Raise(toServer);
                        if (toServer.Filter)
                            return 1;
                        if (toServer.Changed)
                            return 2;
                        break;
                }
            }
            catch (Exception ex) { OnException(ex); }
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