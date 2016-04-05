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
        protected SynchronizationContext context;

        public event EventHandler OnActivate;
        public event RoutedEventHandler OnAdd;

        public ListViewDialog(SynchronizationContext context)
        {
            this.context = context;
        }

        public ViewManager ViewManager { get; set; }

        public CommandBar CommandBar { get; set; }

        public virtual void Activate()
        {
            if(OnActivate != null)
            {
                OnActivate(this, EventArgs.Empty);
            }
        }

    }

    public class FeedDialog :  ListViewDialog
    {
        public FeedDialog(SynchronizationContext context) : base(context)
        {
        }        

        //public override async void OnAdd()
        //{
        //    FeedUrlDialog dlg = new FeedUrlDialog();
        //    await dlg.ShowAsync();
        //    //View.ItemsSource = FeedManager.Feeds;
        //    if (SetDataSoure != null)
        //    {
        //        SetDataSoure(this, FeedManager.Feeds);
        //    }
        //}

        //public override void OnItemClick(object item)
        //{
        //    Feed feed = item as Feed;
        //    if (feed != null)
        //    {
        //        FeedManager.CurrentFeedId = feed.Id;
        //        ViewManager.SwitchTo(PivotView.FeedItems);
        //    }
        //}

        //public override void ItemClick(object sender, ItemClickEventArgs e)
        //{
        //    Feed feed = e.ClickedItem as Feed;
        //    if (feed != null)
        //    {
        //        FeedManager.CurrentFeedId = feed.Id;
        //        ViewManager.SwitchTo(PivotView.FeedItems);
        //    }
        //}
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

        //public override void Activate()
        //{
        //    base.Activate();

        //    //View.ItemsSource = FeedManager.FeedItems;

        //    if (SetDataSoure != null)
        //    {
        //        SetDataSoure(this, FeedManager.FeedItems);
        //    }

        //    CommandBar.PrimaryCommands.Clear();
        //    CommandBar.PrimaryCommands.Add(multiSelectButton);
        //    CommandBar.PrimaryCommands.Add(searchButton);
        //}

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
