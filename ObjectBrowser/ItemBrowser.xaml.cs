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

            World.Items.Added += Items_Added;
            World.Items.Removed += Items_Removed;
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

        private void Items_Added(object sender, CollectionChangedEventArgs<Item> e)
        {
            Dispatcher.Invoke(() =>
            {
                foreach (Item i in e)
                    list.Items.Add(i);
            });
        }

        private void Items_Removed(object sender, CollectionChangedEventArgs<Item> e)
        {
            Dispatcher.Invoke(() =>
            {
                foreach (Item i in e)
                {
                    list.Items.Remove(i);
                    if (list.SelectedItem == i)
                        list.SelectedItem = null;
                }
            });
        }

        private void Item_Changed(object sender, EventArgs e)
        { Dispatcher.Invoke(() => { text.Text = sender.ToString(); }); }
    }
}
