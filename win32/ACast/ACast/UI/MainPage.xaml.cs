using DataBinding;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading;
using System.Threading.Tasks;
using System.Xml;
using System.Xml.Linq;
using System.Xml.Serialization;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;
using Windows.UI.Xaml.Data;
using Windows.UI.Xaml.Input;
using Windows.UI.Xaml.Media;
using Windows.UI.Xaml.Media.Imaging;
using Windows.UI.Xaml.Navigation;
using Windows.Web.Syndication;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace ACast
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private SynchronizationContext context;
        private int currentFeedIdx = 0;
        private GeneratorIncrementalLoadingClass<FeedDetailsListViewItem> feedItems;

        public MainPage()
        {
            this.InitializeComponent();

            context = SynchronizationContext.Current;

            this.NavigationCacheMode = NavigationCacheMode.Required;


            feedListView.ItemClick += feedListView_ItemClick;

            FeedManager.Instance.FeedListLoadedAsync += feedListLoadedAsync;            
            FeedManager.Instance.LoadFeedListAsync();

            Player.Instance.StateChanged += playerStateChanged;
            playButton.Visibility = Visibility.Collapsed;
            playButton.Click += playButton_Click;

            addFeedButton.Click += addFeedButton_Click;
            removeFeedButton.Click += removeFeedButton_Click;
            clearAllButton.Click += cleanallButton_Click;

            playerControlButton.Click += playerControlButton_Click;
            editFeedsButton.Click += editFeedsButton_Click;

            refreshButton.Click += RefreshButton_Click;

        }        

        async private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            await FeedManager.Instance.UpdateFeedAsync(FeedManager.Instance.CurrentFeed.Uri.ToString(), FeedManager.Instance.CurrentFeed);

            FeedManager.Instance.FeedActivatedAsync += feedActivatedAsync;

            FeedManager.Instance.ActiveFeedAsync(currentFeedIdx);
        }

        async void cleanallButton_Click(object sender, RoutedEventArgs e)
        {
            StorageFolder folder = ApplicationData.Current.LocalFolder;
            foreach (var file in await folder.GetFilesAsync())
            {
                await file.DeleteAsync();
            }

            var dialog = new MessageDialog("All files deleted");
            await dialog.ShowAsync();
        }

        void playerControlButton_Click(object sender, RoutedEventArgs e)
        {
            CustomFlyout flyout = playerControlButton.Flyout as CustomFlyout;
            if (flyout != null)
            {
                if (flyout.IsOpen)
                {
                    flyout.Hide();
                }
            }
        }

        void editFeedsButton_Click(object sender, RoutedEventArgs e)
        {
            CustomFlyout flyout = editFeedsButton.Flyout as CustomFlyout;
            if (flyout != null)
            {
                if (flyout.IsOpen)
                {
                    flyout.Hide();
                }
            }
        }

        async void addFeedButton_Click(object sender, RoutedEventArgs e)
        {
            var newFeedUrlDlg = new FeedUrlDialog();
            await newFeedUrlDlg.ShowAsync();

            FeedManager.Instance.FeedListLoadedAsync += feedListLoadedAsync;
            FeedManager.Instance.LoadFeedListAsync();
        }

        void removeFeedButton_Click(object sender, RoutedEventArgs e)
        {
            FeedManager.Instance.FeedListDeletedAsync += FeedListDeletedAsync;
            FeedManager.Instance.DeleteFeedList();
        }

        void playerStateChanged(object sender, Windows.Media.Playback.MediaPlayerState e)
        {
            context.Post(new SendOrPostCallback((o) =>
            {
                Debug.WriteLine("player state changed:" + e.ToString());
                switch (e)
                {
                    case MediaPlayerState.Buffering:
                        break;
                    case MediaPlayerState.Closed:
                        playButton.Visibility = Visibility.Collapsed;
                        break;
                    case MediaPlayerState.Opening:
                        playButton.Visibility = Visibility.Visible;
                        break;
                    case MediaPlayerState.Paused:
                        playButton.Icon = new SymbolIcon(Symbol.Play);
                        break;
                    case MediaPlayerState.Playing:
                        playButton.Icon = new SymbolIcon(Symbol.Pause);
                        break;
                    case MediaPlayerState.Stopped:
                        playButton.Visibility = Visibility.Collapsed;
                        break;
                    default:
                        break;
                }
            }), null);
        }

        private void feedListLoadedAsync()
        {
            FeedManager.Instance.FeedListLoadedAsync -= feedListLoadedAsync;

            context.Post(new SendOrPostCallback((o) =>
            {
                feedListView.Items.Clear();

                foreach (Feed item in FeedManager.Instance.FeedList)
                {
                    FeedListViewItem customItem = new FeedListViewItem();
                    customItem.SetItem(item);
                    feedListView.Items.Add(customItem);
                }
            }), null);
        }

        private void feedActivatedAsync()
        {
            feedItems = new GeneratorIncrementalLoadingClass<FeedDetailsListViewItem>(
                (uint)FeedManager.Instance.CurrentFeedItems.Count, 
                (count) => {
                    var feedItem = FeedManager.Instance.CurrentFeedItems[count];
                    return new FeedDetailsListViewItem(feedItem);
                }
            );
            feedItemsListView.ItemsSource = feedItems;           
        }

        /// <summary>
        /// Invoked when this page is about to be displayed in a Frame.
        /// </summary>
        /// <param name="e">Event data that describes how this page was reached.
        /// This parameter is typically used to configure the page.</param>
        protected override void OnNavigatedTo(NavigationEventArgs e)
        {
            // TODO: Prepare page for display here.

            // TODO: If your application contains multiple pages, ensure that you are
            // handling the hardware Back button by registering for the
            // Windows.Phone.UI.Input.HardwareButtons.BackPressed event.
            // If you are using the NavigationHelper provided by some templates,
            // this event is handled for you.
        }

        private void FeedListDeletedAsync()
        {
            FeedManager.Instance.FeedListDeletedAsync -= FeedListDeletedAsync;
            FeedManager.Instance.FeedListLoadedAsync += feedListLoadedAsync;
            FeedManager.Instance.LoadFeedListAsync();
        }

        private void feedListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            FeedManager.Instance.FeedActivatedAsync += feedActivatedAsync;

            int feedIdx = feedListView.Items.IndexOf(e.ClickedItem);
            FeedManager.Instance.ActiveFeedAsync(feedIdx);

            currentFeedIdx = feedIdx;

        }

        private void playButton_Click(object sender, RoutedEventArgs e)
        {
            switch (Player.Instance.State)
            {
                case MediaPlayerState.Paused:
                    Player.Instance.Resume();
                    break;
                case MediaPlayerState.Playing:
                    Player.Instance.Pause();
                    break;
                default:
                    break;
            }
        }

    }

    public class CustomFlyout : Flyout
    {
        public bool IsOpen { get; private set; }

        public CustomFlyout()
        {
            this.Opened += OnOpened;
            this.Closed += OnClosed;
        }

        void OnClosed(object sender, object e)
        {
            IsOpen = false;
        }

        void OnOpened(object sender, object e)
        {
            IsOpen = true;    
        }
    }

}
