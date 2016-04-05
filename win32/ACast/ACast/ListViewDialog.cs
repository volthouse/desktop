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
    public enum PivotView {
        Feeds,
        FeedItems,
        Player
    }

    public class ViewManager
    {
        private Pivot pivot;

        public List<ListViewDialog> Dialogs = new List<ListViewDialog>();

        public Pivot Pivot {
            get { return pivot; }
            set
            {
                if (pivot != null)
                {
                    pivot.SelectionChanged -= pivot_SelectionChanged;
                }
                pivot = value;
                pivot.SelectionChanged += pivot_SelectionChanged;
            }
        }

        public void SwitchTo(PivotView view)
        {
            switch (view)
            {
                case PivotView.Feeds:
                    Pivot.SelectedIndex = 0;
                    break;
                case PivotView.FeedItems:
                    Pivot.SelectedIndex = 1;
                    break;
                case PivotView.Player:
                    Pivot.SelectedIndex = 2;
                    break;
                default:
                    break;
            }
        }

        private void pivot_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            Dialogs[pivot.SelectedIndex].Activate();
        }

    }

    public class ListViewDialog
    {
        protected AddButton addButton = new AddButton();
        protected RemoveButton removeButton = new RemoveButton();
        protected MultiSelectButton multiSelectButton = new MultiSelectButton();
        protected SearchButton searchButton = new SearchButton();
        protected CancelButton cancelButton = new CancelButton();
        protected RefreshButton refreshButton = new RefreshButton();

        protected AutoSuggestBox searchTextBox;

        protected ListView listView;
        protected SynchronizationContext context;

        public ListViewDialog(SynchronizationContext context)
        {
            this.context = context;

            addButton.Click += (object sender, Windows.UI.Xaml.RoutedEventArgs e) => { OnAdd(); };
            removeButton.Click += (object sender, Windows.UI.Xaml.RoutedEventArgs e) => { OnRemove(); };
            refreshButton.Click += (object sender, Windows.UI.Xaml.RoutedEventArgs e) => { OnRefresh(); };
            searchButton.Click += (object sender, Windows.UI.Xaml.RoutedEventArgs e) => { OnSearchButtonClick(); };
            cancelButton.Click += (object sender, Windows.UI.Xaml.RoutedEventArgs e) => { OnCancelButtonClick(); };
        }

        public ViewManager ViewManager { get; set; }

        public CommandBar CommandBar { get; set; }

        public AutoSuggestBox SearchTextBox {
            get { return searchTextBox; }
            set
            {
                if (searchTextBox != null)
                {
                    searchTextBox.KeyDown -= searchTextBox_KeyDown;
                }
                searchTextBox = value;
                searchTextBox.Visibility = Visibility.Collapsed;
                searchTextBox.KeyDown += searchTextBox_KeyDown;
            }
        }

        public virtual ListView View {
            get { return listView; }
            set {
                listView = value;
                listView.ItemClick += VievItemClick;
            }
        }

        public virtual void Activate()
        {            
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

        public virtual void OnSearchButtonClick()
        {
            CommandBar.PrimaryCommands.Remove(searchButton);
            CommandBar.PrimaryCommands.Add(cancelButton);
            searchTextBox.Visibility = Visibility.Visible;
        }

        public virtual void OnCancelButtonClick()
        {
            if (searchTextBox.Visibility == Visibility.Visible)
            {
                searchTextBox.Visibility = Visibility.Collapsed;
                CommandBar.PrimaryCommands.Remove(cancelButton);
                CommandBar.PrimaryCommands.Add(searchButton);
            }
        }

        public virtual void OnSearch(string searchString)
        {

        }

        public virtual void OnItemClick(object item)
        {

        }

        private void VievItemClick(object sender, ItemClickEventArgs e)
        {
            OnItemClick(e.ClickedItem);
        }

        void searchTextBox_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                OnSearch(searchTextBox.Text);
            }
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

            CommandBar.PrimaryCommands.Clear();
            CommandBar.PrimaryCommands.Add(addButton);
            CommandBar.PrimaryCommands.Add(removeButton);
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
                ViewManager.SwitchTo(PivotView.FeedItems);
            }
        }
    }

    public class FeedItemsDialog : ListViewDialog
    {
        private AppBarButton pickerButton;
        private AppBarButton playButton;

        public FeedItemsDialog(SynchronizationContext context) : base(context)
        {
        }

        public AppBarButton PickerButton {
            get { return pickerButton; }
            set
            {
                if (pickerButton != null)
                {
                    pickerButton.Click -= pickerButton_Click;
                }

                pickerButton = value;

                pickerButton.Click += pickerButton_Click;
            }
        }

        public AppBarButton PlayButton
        {
            get { return playButton; }
            set
            {
                if (playButton != null)
                {
                    playButton.Click -= playButton_Click;
                }

                playButton = value;

                playButton.Click += playButton_Click;
            }
        } 

        public override void Activate()
        {
            base.Activate();

            View.ItemsSource = FeedManager.FeedItems;

            CommandBar.PrimaryCommands.Clear();
            CommandBar.PrimaryCommands.Add(multiSelectButton);
            CommandBar.PrimaryCommands.Add(searchButton);
        }

        private void pickerButton_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton button = e.OriginalSource as AppBarButton;
            if (button != null)
            {
                FeedItem feedItem = button.DataContext as FeedItem;
                if (feedItem != null)
                {
                    FeedManager.Instance.DownloadFeedItemMedia(feedItem);
                }
            }
        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            AppBarButton button = e.OriginalSource as AppBarButton;
            if (button != null)
            {
                FeedItem feedItem = button.DataContext as FeedItem;
                if (feedItem != null)
                {
                    Player.Instance.Play(feedItem);
                }
            }
        }
    }

}
