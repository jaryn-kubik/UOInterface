using System;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace UOInterface
{
    public static class Extensions
    {
        public static void Raise(this EventHandler handler, object sender = null) { Raise(handler, EventArgs.Empty, sender); }
        public static void Raise(this EventHandler handler, EventArgs e, object sender = null)
        {
            if (handler != null)
                handler(sender, e);
        }

        public static void Raise<T>(this EventHandler<T> handler, T e, object sender = null)
        {
            if (handler != null)
                handler(sender, e);
        }

        public static void RaiseAsync(this EventHandler handler, object sender = null) { RaiseAsync(handler, EventArgs.Empty, sender); }
        public static void RaiseAsync(this EventHandler handler, EventArgs e, object sender = null)
        {
            if (handler != null)
                Task.Run(() =>
                {
                    try { handler(sender, e); }
                    catch (Exception ex) { MessageBox.Show(ex.ToString()); }
                });
        }

        public static void RaiseAsync<T>(this EventHandler<T> handler, T e, object sender = null)
        {
            if (handler != null)
                Task.Run(() =>
                {
                    try { handler(sender, e); }
                    catch (Exception ex) { MessageBox.Show(ex.ToString()); }
                });
        }
    }
}