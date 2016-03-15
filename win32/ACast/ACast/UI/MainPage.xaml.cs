﻿using ACast.DataBinding;
using ACastShared;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Threading.Tasks;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Popups;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Navigation;
using System.Linq;

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

        private SearchFeedButton searchFeedDetailsButton;
        private AddFeedButton addFeedButton;
        private RefreshFeedButton refreshButton;
        private RemoveFeedButton removeButton;
        private SelectButton selectButton;
        private CancelButton cancelButton;

        public static MainPage Instance;

        public MainPage()
        {
            Player.Instance = new Player();

            Instance = this;

            this.InitializeComponent();

            context = SynchronizationContext.Current;

            this.NavigationCacheMode = NavigationCacheMode.Required;
            
            feedListView.ItemClick += feedListView_ItemClick;
            pivot.PivotItemLoaded += pivot_PivotItemLoaded;

            addFeedButton = new AddFeedButton();
            addFeedButton.Click += addFeedButton_Click;

            refreshButton = new RefreshFeedButton();
            refreshButton.Click += refreshButton_Click;

            removeButton = new RemoveFeedButton();
            removeButton.Click += cleanAllButton_Click;

            selectButton = new SelectButton();
            selectButton.Click += selectButton_Click;

            cancelButton = new CancelButton();
            cancelButton.Click += cancelButton_Click;
            searchFeedDetailsButton = new SearchFeedButton(serachFeedDetailsBox);
            searchFeedDetailsButton.SearchChanged += searchFeedDetailsButton_SearchChanged;
            searchFeedDetailsButton.SearchCanceled += searchFeedDetailsButton_SearchCanceled;

            serachFeedDetailsBox.Visibility = Visibility.Collapsed;

            feedItemsListView.ItemClick += feedItemsListView_ItemClick;

            FeedManager.Instance.DeserializeFeedsAsync(feedListLoadedAsync);            
        }

        void feedItemsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            feedItemsListView.SelectedItems.Add(e.ClickedItem);
        }

        void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (pivot.SelectedItem.Equals(feedDetailsPivotItem))
            {
                if (feedItemsListView.SelectionMode != ListViewSelectionMode.None)                
                {
                    feedItemsListView.SelectionMode = ListViewSelectionMode.Single;
                    this.commandBar.PrimaryCommands.Clear();
                    this.commandBar.PrimaryCommands.Add(refreshButton);
                    this.commandBar.PrimaryCommands.Add(selectButton);
                    this.commandBar.PrimaryCommands.Add(searchFeedDetailsButton);
                    this.commandBar.Visibility = Visibility.Visible;
                }
            }
        }

        public void Play(FeedItem feedItem)
        {
            pivot.SelectedItem = playerPivotItem;
            playerControl.Play(feedItem);
        }

        void selectButton_Click(object sender, RoutedEventArgs e)
        {
            if (pivot.SelectedItem.Equals(feedDetailsPivotItem))
            {
                if (feedItemsListView.SelectionMode == ListViewSelectionMode.Single)
                {
                    //if (feedItemsListView.SelectedItems.Count > 0)
                    //    feedItemsListView.SelectedItems.Clear();
                    feedItemsListView.SelectionMode = ListViewSelectionMode.Multiple;
                    this.commandBar.PrimaryCommands.Clear();
                    this.commandBar.PrimaryCommands.Add(cancelButton);
                    this.commandBar.PrimaryCommands.Add(removeButton);
                }               
            }
        }

        void searchFeedDetailsButton_SearchCanceled(object sender, EventArgs e)
        {
            updateFeedItemsListView(null); ;
        }

        void searchFeedDetailsButton_SearchChanged(object sender, IList<FeedItem> e)
        {
            List<FeedDetailsListViewItem> items = new List<FeedDetailsListViewItem>();
 	        foreach (var item in e)
	        {
		        items.Add(new FeedDetailsListViewItem(item));
	        }

            feedItemsListView.ItemsSource = items;
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

            Application.Current.Suspending += app_Suspending;
            Application.Current.Resuming += app_Resuming;

            var currentTrackId = ApplicationSettingsHelper.ReadResetSettingsValue(ApplicationSettingsConstants.TrackId);
            var currentTrackPosition = ApplicationSettingsHelper.ReadResetSettingsValue(ApplicationSettingsConstants.Position);
            if (currentTrackId != null)
                DebugService.Add(currentTrackId.ToString());
            if (currentTrackPosition != null)
                DebugService.Add(currentTrackPosition.ToString());
        }

        private void app_Resuming(object sender, object e)
        {
            
            Player.Instance.ForegroundAppResuming();
        }

        private void app_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            Player.Instance.ForegroundAppSuspending(e.SuspendingOperation.GetDeferral());
        }

        void pivot_PivotItemLoaded(Pivot sender, PivotItemEventArgs args)
        {
            if (args.Item.Equals(feedsPivotItem))
            {
                this.commandBar.PrimaryCommands.Clear();
                this.commandBar.PrimaryCommands.Add(addFeedButton);
                this.commandBar.PrimaryCommands.Add(removeButton);
                this.commandBar.Visibility = Visibility.Visible;
            }
            else if (args.Item.Equals(feedDetailsPivotItem))
            {
                this.commandBar.PrimaryCommands.Clear();
                this.commandBar.PrimaryCommands.Add(refreshButton);
                this.commandBar.PrimaryCommands.Add(selectButton);
                this.commandBar.PrimaryCommands.Add(searchFeedDetailsButton);
                this.commandBar.Visibility = Visibility.Visible;
            }
            else if (args.Item.Equals(playerPivotItem))
            {
                playerControl.Activate();
                this.commandBar.PrimaryCommands.Clear();
                this.commandBar.Visibility = Visibility.Collapsed;
            }
            else if (args.Item.Equals(debugPivotItem))
            {
                this.commandBar.PrimaryCommands.Clear();
                this.commandBar.PrimaryCommands.Add(refreshButton);

                debugList.Items.Clear();
                foreach (var item in DebugService.Instance.DebugMessages)
                {
                    debugList.Items.Add(item);
                }
            }
        }

        async private void refreshButton_Click(object sender, RoutedEventArgs e)
        {
            if (pivot.SelectedItem == debugPivotItem)
            {
                debugList.Items.Clear();
                foreach (var item in DebugService.Instance.DebugMessages)
                {
                    debugList.Items.Add(item);
                }
            }
            else
            {
                await FeedManager.Instance.UpdateFeedAsync(
                    FeedManager.Instance.CurrentFeed.Uri.ToString(), FeedManager.Instance.CurrentFeed, true
                );

                FeedManager.Instance.ActivateFeedAsync(currentFeedIdx, feedActivatedAsync);
            }
        }

        async void cleanAllButton_Click(object sender, RoutedEventArgs e)
        {
            if (pivot.SelectedItem.Equals(feedsPivotItem))
            {
                StorageFolder folder = ApplicationData.Current.LocalFolder;
                foreach (var file in await folder.GetFilesAsync())
                {
                    await file.DeleteAsync();
                }

                MessageDialog dlg = new MessageDialog("All deleted");
                await dlg.ShowAsync();
            }
            else if (pivot.SelectedItem.Equals(feedDetailsPivotItem))
            {
                foreach (var item in feedItemsListView.SelectedItems.Cast <FeedDetailsListViewItem>())
                {
                    await item.FeedItem.DeleteMediaFile();
                }
                FeedManager.Instance.CurrentFeed.Serialize();
            }
        }

        async void addFeedButton_Click(object sender, RoutedEventArgs e)
        {
            var newFeedUrlDlg = new FeedUrlDialog();
            await newFeedUrlDlg.ShowAsync();

            FeedManager.Instance.DeserializeFeedsAsync(feedListLoadedAsync);
        }             

        private void feedListLoadedAsync(object sender)
        {
            context.Post(new SendOrPostCallback((o) =>
            {
                feedListView.Items.Clear();

                foreach (Feed item in FeedManager.Instance.Feeds)
                {
                    FeedListViewItem customItem = new FeedListViewItem();
                    customItem.SetItem(item);
                    feedListView.Items.Add(customItem);
                }
            }), null);
        } 

        void feedActivatedAsync(object sender)
        {
            context.Post(updateFeedItemsListView, null);
        }

        private async void updateFeedItemsListView(object state)
        {
            feedItemsListView.ItemsSource = new FeedItemsIncrementalLoading();
            await feedItemsListView.LoadMoreItemsAsync();

            pivot.SelectedItem = feedDetailsPivotItem;
        }

        private void feedListDeletedAsync(object sender)
        {
            FeedManager.Instance.DeserializeFeedsAsync(feedListLoadedAsync);
        }

        private void feedListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            int feedIdx = feedListView.Items.IndexOf(e.ClickedItem);
            FeedManager.Instance.ActivateFeedAsync(feedIdx, feedActivatedAsync);

            currentFeedIdx = feedIdx;
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

    public class AddFeedButton : AppBarButton
    {
        public AddFeedButton()
        {
            Icon = new SymbolIcon(Symbol.Add);
        }
    }

    public class RemoveFeedButton : AppBarButton
    {
        public RemoveFeedButton()
        {
            Icon = new SymbolIcon(Symbol.Remove);
        }
    }

    public class RefreshFeedButton : AppBarButton
    {
        public RefreshFeedButton()
        {
            Icon = new SymbolIcon(Symbol.Refresh);
        }
    }

    public class SelectButton : AppBarButton
    {
        public SelectButton()
        {
            Icon = new SymbolIcon(Symbol.Bullets);
        }
    }

    public class CancelButton : AppBarButton
    {
        public CancelButton()
        {
            Icon = new SymbolIcon(Symbol.Cancel);
        }
    }

    public class SearchFeedButton : AppBarButton
    {
        private AutoSuggestBox textBox;

        public event EventHandler<IList<FeedItem>> SearchChanged;

        public event EventHandler SearchCanceled;

        public SearchFeedButton(AutoSuggestBox textBox)
        {
            this.textBox = textBox;
            this.textBox.KeyDown += textBox_KeyDown;
            //this.textBox.
            Icon = new SymbolIcon(Symbol.Zoom);
            this.Click += SearchFeedButton_Click;
        }

        //void textBox_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    Windows.UI.ViewManagement.InputPane.GetForCurrentView().TryShow();
        //}

        private void SearchFeedButton_Click(object sender, RoutedEventArgs e)
        {
            switch (textBox.Visibility)
            {
                case Visibility.Collapsed:
                    Icon = new SymbolIcon(Symbol.Cancel);
                    textBox.Visibility = Visibility.Visible;
                    //Windows.UI.Xaml.Input.FocusManager.
                    
                       
                    break;
                case Visibility.Visible:
                    Icon = new SymbolIcon(Symbol.Zoom);
                    textBox.Visibility = Visibility.Collapsed;
                    if (SearchCanceled != null)
                    {
                        SearchCanceled(this, EventArgs.Empty);
                    }
                    break;
                default:
                    break;
            }
        }

        private void textBox_KeyDown(object sender, Windows.UI.Xaml.Input.KeyRoutedEventArgs e)
        {
            if (e.Key == Windows.System.VirtualKey.Enter)
            {
                textBox.Items.Add(textBox.Text);                
                //textBox.Visibility = Visibility.Collapsed;
                //Icon = new SymbolIcon(Symbol.Zoom);
                //Windows.UI.ViewManagement.InputPane.GetForCurrentView().TryHide();
                //textBox.Focus(FocusState.Unfocused);
                search(textBox.Text);                
            }
        }

        private void search(string searchText)
        {
            var currentFeedItems = from item in FeedManager.Instance.CurrentFeed.Items where item.Summary.Contains(searchText) select item;

            List<FeedItem> items = new List<FeedItem>(currentFeedItems);
            if (SearchChanged != null)
            {
                SearchChanged(this, items);
            }
        }
    }
}
