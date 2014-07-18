using System;
using System.Windows.Controls;
using UOInterface;

namespace ObjectBrowser
{
    public partial class ItemBrowser
    {
        public ItemBrowser()
        {
            InitializeComponent();

            World.ItemAdded += World_ItemAdded;
            World.ItemRemoved += World_ItemRemoved;
            World.Cleared += World_Cleared;
            list.SelectionChanged += list_SelectionChanged;
        }

        private void list_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            foreach (Item i in e.RemovedItems)
            {
                i.AppearanceChanged += Item_Changed;
                i.AttributesChanged += Item_Changed;
                i.PositionChanged += Item_Changed;
                i.OwnerChanged += Item_Changed;
                text.Clear();
            }
            foreach (Item i in e.AddedItems)
            {
                i.AppearanceChanged -= Item_Changed;
                i.AttributesChanged -= Item_Changed;
                i.PositionChanged -= Item_Changed;
                i.OwnerChanged -= Item_Changed;
                Item_Changed(i, EventArgs.Empty);
            }
            e.Handled = true;
        }

        private void World_ItemAdded(object sender, Item e)
        {
            Dispatcher.Invoke(() =>
            {
                e.AppearanceChanged += Item_AppearanceChanged;
                list.Items.Add(e);
            });
        }

        private void World_ItemRemoved(object sender, Item e)
        {
            Dispatcher.Invoke(() =>
            {
                list.Items.Remove(e);
                e.AppearanceChanged -= Item_AppearanceChanged;
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

        private void Item_AppearanceChanged(object sender, EventArgs e)
        { Dispatcher.Invoke(() => list.Items.Refresh()); }

        private void Item_Changed(object sender, EventArgs e)
        { Dispatcher.Invoke(() => { text.Text = sender.ToString(); }); }
    }
}
