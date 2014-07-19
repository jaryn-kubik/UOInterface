using System.Windows.Input;
using UOInterface;
using UOInterface.Network;

namespace ObjectBrowser
{
    public partial class Events
    {
        public Events()
        {
            InitializeComponent();

            Client.Connected += (s, e) => AppendLine("Connected");
            Client.Disconnecting += (s, e) => AppendLine("Disconnecting");
            Client.Closing += (s, e) => AppendLine("Closing");

            Client.FocusChanged += (s, e) => AppendLine("FocusChanged - " + e);
            Client.VisibilityChanged += (s, e) => AppendLine("VisibilityChanged - " + e);

            Client.KeyDown += Client_KeyDown;
            Client.PacketToClient += Client_PacketToClient;
            Client.PacketToServer += Client_PacketToServer;
        }

        private void AppendLine(string line)
        {
            Dispatcher.InvokeAsync(() => AppendLineInvoked(line));
        }

        private void AppendLineInvoked(string line)
        {
            text.AppendText(line + "\n");
            text.ScrollToEnd();
        }

        private void Client_KeyDown(object sender, UOKeyEventArgs e)
        {
            Key k = KeyInterop.KeyFromVirtualKey(e.VirtualCode);
            AppendLine(string.Format("KeyDown - {0} - {1}", k, (ModifierKeys)e.Modifiers));
        }

        private void Client_PacketToClient(object sender, Packet p)
        { AppendLine(string.Format("PacketToClient - {0:X2} - {1} bytes", p.ID, p.Length)); }

        private void Client_PacketToServer(object sender, Packet p)
        { AppendLine(string.Format("PacketToServer - {0:X2} - {1} bytes", p.ID, p.Length)); }
    }
}
