using ACast.Database;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;

namespace ACast
{
    public class ListViewDialog
    {
        protected AddButton addButton = new AddButton();
        protected RemoveButton removeButton = new RemoveButton();
        protected MultiSelectButton multiSelectButton = new MultiSelectButton();
        protected SearchButton searchButton = new SearchButton();
        protected CancelButton cancelButton = new CancelButton();
        protected RefreshButton refreshButton = new RefreshButton();

        protected ListView listView;
        protected SynchronizationContext context;

        public ListViewDialog(SynchronizationContext context)
        {
            this.context = context;

            addButton.Click += (object sender, Windows.UI.Xaml.RoutedEventArgs e) => { OnAdd(); };
            addButton.Click += (object sender, Windows.UI.Xaml.RoutedEventArgs e) => { OnRemove(); };
            refreshButton.Click += (object sender, Windows.UI.Xaml.RoutedEventArgs e) => { OnRefresh(); };
        }

        public CommandBar CommandBar { get; set; }

        public ListView View {
            get { return listView; }
            set {
                listView = value;
                listView.ItemClick += VievItemClick;
            }
        }

        private void VievItemClick(object sender, ItemClickEventArgs e)
        {
            OnItemClick(e.ClickedItem);
        }


        public virtual void Activate()
        {
            CommandBar.PrimaryCommands.Clear();
            CommandBar.PrimaryCommands.Add(addButton);
            CommandBar.PrimaryCommands.Add(removeButton);


            
        }

        public virtual void OnLoaded()
        {
            //View.ItemsSource = listController.List;
        }

        public virtual void Deactivate()
        {

        }

        public virtual void OnAdd()
        {

        }

        public virtual void OnRemove()
        {

        }

        public virtual void OnRefresh()
        {

        }

        public virtual void OnItemClick(object item)
        {

        }
    }

    public class FeedDialog :  ListViewDialog
    {
        public FeedDialog(SynchronizationContext context) : base(context)
        {
        }

        public override void Activate()
        {
            base.Activate();
            View.ItemsSource = FeedManager.Feeds;
        }

        public override async void OnAdd()
        {
            FeedUrlDialog dlg = new FeedUrlDialog();
            await dlg.ShowAsync();
            View.ItemsSource = FeedManager.Feeds;
        }

        public override void OnItemClick(object item)
        {
            Feed feed = item as Feed;
            if (feed != null)
            {
                FeedManager.CurrentFeedId = feed.Id;
            }
        }

    }

    public class FeedItemsDialog : ListViewDialog
    {
        public FeedItemsDialog(SynchronizationContext context) : base(context)
        {
        }

        public override void Activate()
        {
            base.Activate();
            View.ItemsSource = FeedManager.FeedItems;

        }
    }

}
