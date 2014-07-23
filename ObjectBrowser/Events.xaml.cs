using System.Windows.Input;
using UOInterface;

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

            World.Cleared += (s, e) => AppendLine("World cleared.");
            World.MapChanged += (s, e) => AppendLine("Map changed.");
            World.ItemsChanged += (s, e) => AppendLine(string.Format("ItemsChanged - {0} added, {1} removed.",
                                                                        e.Added.Count, e.Removed.Count));
            World.MobilesChanged += (s, e) => AppendLine(string.Format("MobilesChanged - {0} added, {1} removed.",
                                                                        e.Added.Count, e.Removed.Count));

            Chat.Message += Chat_Message;
            Chat.LocalizedMessage += Chat_LocalizedMessage;
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

        private void Chat_Message(object sender, UOMessageEventArgs e)
        {
            Entity entity = (Entity)sender;
            AppendLine(string.Format("Message - {0}: '{1}'", entity.Name, e.Text));
        }

        private void Chat_LocalizedMessage(object sender, UOMessageEventArgs e)
        {
            Entity entity = (Entity)sender;
            if (string.IsNullOrEmpty(e.Text))
                AppendLine(string.Format("Message - {0}: {1}", entity.Name, e.Cliloc));
            else
                AppendLine(string.Format("Message - {0}: {1}, '{2}'", entity.Name, e.Cliloc, e.Text));
        }
    }
}
