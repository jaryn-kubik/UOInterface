using System;
using System.Windows.Controls;
using UOInterface;

namespace ObjectBrowser
{
    public partial class MobileBrowser
    {
        public MobileBrowser()
        {
            InitializeComponent();

            World.MobileAdded += World_MobileAdded;
            World.MobileRemoved += World_MobileRemoved;
            World.Cleared += World_Cleared;
            list.SelectionChanged += list_SelectionChanged;
        }

        private void list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (Mobile m in e.RemovedItems)
            {
                m.AppearanceChanged -= Mobile_Changed;
                m.AttributesChanged -= Mobile_Changed;
                m.HitsChanged -= Mobile_Changed;
                m.LayerChanged -= Mobile_Changed;
                m.ManaChanged -= Mobile_Changed;
                m.Moved -= Mobile_Changed;
                m.StaminaChanged -= Mobile_Changed;
                var p = m as PlayerMobile;
                if (p != null)
                    p.StatsChanged -= Mobile_Changed;
                text.Clear();
            }
            foreach (Mobile m in e.AddedItems)
            {
                m.AppearanceChanged += Mobile_Changed;
                m.AttributesChanged += Mobile_Changed;
                m.HitsChanged += Mobile_Changed;
                m.LayerChanged += Mobile_Changed;
                m.ManaChanged += Mobile_Changed;
                m.Moved += Mobile_Changed;
                m.StaminaChanged += Mobile_Changed;
                var p = m as PlayerMobile;
                if (p != null)
                    p.StatsChanged += Mobile_Changed;
                Mobile_Changed(m, EventArgs.Empty);
            }
            e.Handled = true;
        }

        private void World_MobileAdded(object sender, Mobile e)
        {
            Dispatcher.Invoke(() =>
            {
                e.AppearanceChanged += Mobile_AppearanceChanged;
                list.Items.Add(e);
            });
        }

        private void World_MobileRemoved(object sender, Mobile e)
        {
            Dispatcher.Invoke(() =>
            {
                list.Items.Remove(e);
                e.AppearanceChanged -= Mobile_AppearanceChanged;
                if (list.SelectedItem == e)
                    list.SelectedItem = null;
            });
        }

        private void World_Cleared(object sender, EventArgs e)
        {
            Dispatcher.Invoke(() =>
            {
                list.Items.Clear();
                text.Clear();
            });
        }

        private void Mobile_AppearanceChanged(object sender, EventArgs e)
        { Dispatcher.Invoke(() => list.Items.Refresh()); }

        private void Mobile_Changed(object sender, EventArgs e)
        { Dispatcher.Invoke(() => { text.Text = sender.ToString(); }); }
    }
}
