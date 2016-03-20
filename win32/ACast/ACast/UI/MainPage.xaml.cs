using ACast.DataBinding;
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
using Windows.ApplicationModel.Background;

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

        private SearchFeedButton searchButton;
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
            removeButton.Click += removeButton_Click;

            selectButton = new SelectButton();
            selectButton.Click += selectButton_Click;

            cancelButton = new CancelButton();
            cancelButton.Click += cancelButton_Click;

            searchButton = new SearchFeedButton(serachFeedDetailsBox);
            searchButton.Click += searchButton_Click;
            searchButton.SearchChanged += searchFeedDetailsButton_SearchChanged;

            serachFeedDetailsBox.Visibility = Visibility.Collapsed;

            feedItemsListView.ItemClick += feedItemsListView_ItemClick;

            FeedManager.Instance.DeserializeFeedsAsync(feedListLoadedAsync);            
        }        

        public void Play(FeedItem feedItem)
        {
            pivot.SelectedItem = playerPivotItem;
            playerControl.Play(feedItem);
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
#if true
            DebugService.Add("OnNavigatedTo");

            var currentTrackId = ApplicationSettingsHelper.ReadResetSettingsValue(ApplicationSettingsConstants.TrackId);
            var currentTrackPosition = ApplicationSettingsHelper.ReadResetSettingsValue(ApplicationSettingsConstants.Position);
            if (currentTrackId != null)
                DebugService.Add("Track" + currentTrackId.ToString());
            if (currentTrackPosition != null)
                DebugService.Add("Trackpos:" + currentTrackPosition.ToString());

            //var timerStarted = ApplicationSettingsHelper.ReadResetSettingsValue(ApplicationSettingsConstants.SleepTimerStarted);
            //var timerStopped = ApplicationSettingsHelper.ReadResetSettingsValue(ApplicationSettingsConstants.SleepTimerStopped);
            //if(timerStarted != null)
            //    DebugService.Add("Timer start:" + timerStarted.ToString(), false);
            //if (timerStopped != null)
            //    DebugService.Add("Timer stopp:" + timerStopped.ToString(), false);

            DebugService.Instance.Serialize();

#endif           
        }

        void feedItemsListView_ItemClick(object sender, ItemClickEventArgs e)
        {
            feedItemsListView.SelectedItems.Add(e.ClickedItem);
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
                this.commandBar.PrimaryCommands.Add(searchButton);
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
                this.commandBar.PrimaryCommands.Add(removeButton);
                this.commandBar.Visibility = Visibility.Visible;

                DebugService.Instance.Deserialize();
                debugList.Items.Clear();
                foreach (var item in DebugService.Instance.DebugMessages)
                {
                    debugList.Items.Add(item);
                }
            }
        }

        void searchButton_Click(object sender, RoutedEventArgs e)
        {
            if (pivot.SelectedItem.Equals(feedDetailsPivotItem))
            {
                feedItemsListView.SelectionMode = ListViewSelectionMode.Single;
                this.commandBar.PrimaryCommands.Clear();
                this.commandBar.PrimaryCommands.Add(selectButton);
                this.commandBar.PrimaryCommands.Add(cancelButton);
                this.commandBar.Visibility = Visibility.Visible;

                searchButton.Active = true;
            }
        }

        void cancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (pivot.SelectedItem.Equals(feedDetailsPivotItem))
            {
                if (feedItemsListView.SelectionMode == ListViewSelectionMode.Single)                
                {
                    if (searchButton.Active)
                    {
                        this.commandBar.PrimaryCommands.Clear();
                        this.commandBar.PrimaryCommands.Add(refreshButton);
                        this.commandBar.PrimaryCommands.Add(selectButton);
                        this.commandBar.PrimaryCommands.Add(searchButton);
                        this.commandBar.Visibility = Visibility.Visible;

                        searchButton.Active = false;
                    }
                    else
                    {
                        this.commandBar.PrimaryCommands.Clear();
                        this.commandBar.PrimaryCommands.Add(refreshButton);
                        this.commandBar.PrimaryCommands.Add(selectButton);
                        this.commandBar.PrimaryCommands.Add(searchButton);
                        this.commandBar.Visibility = Visibility.Visible;
                    }
                }
                else
                {
                    if (searchButton.Active)
                    {
                        this.commandBar.PrimaryCommands.Clear();
                        this.commandBar.PrimaryCommands.Add(selectButton);
                        this.commandBar.PrimaryCommands.Add(cancelButton);
                        this.commandBar.Visibility = Visibility.Visible;

                    }
                    else
                    {
                        this.commandBar.PrimaryCommands.Clear();
                        this.commandBar.PrimaryCommands.Add(refreshButton);
                        this.commandBar.PrimaryCommands.Add(selectButton);
                        this.commandBar.PrimaryCommands.Add(searchButton);
                        this.commandBar.Visibility = Visibility.Visible;
                    }

                    feedItemsListView.SelectionMode = ListViewSelectionMode.Single;
                }
            }
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
                    this.commandBar.PrimaryCommands.Add(removeButton);
                    this.commandBar.PrimaryCommands.Add(cancelButton);
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

        async void removeButton_Click(object sender, RoutedEventArgs e)
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
                if (feedItemsListView.SelectedItems.Count > 0)
                {
                    foreach (var item in feedItemsListView.SelectedItems.Cast<FeedDetailsListViewItem>())
                    {
                        await item.FeedItem.DeleteMediaFile();
                    }
                    FeedManager.Instance.CurrentFeed.Serialize();

                    feedItemsListView.SelectedItems.Clear();
                    feedItemsListView.SelectionMode = ListViewSelectionMode.Single;

                    this.commandBar.PrimaryCommands.Clear();
                    this.commandBar.PrimaryCommands.Add(refreshButton);
                    this.commandBar.PrimaryCommands.Add(selectButton);
                    this.commandBar.PrimaryCommands.Add(searchButton);
                    this.commandBar.Visibility = Visibility.Visible;
                }
            }
            else if (pivot.SelectedItem.Equals(debugPivotItem))
            {
                DebugService.Clear();
                debugList.Items.Clear();
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
            Icon = new SymbolIcon(Symbol.Delete);
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

    public class SearchButton : AppBarButton
    {
        public SearchButton()
        {
            Icon = new SymbolIcon(Symbol.Zoom);
        }
    }

    public class SearchFeedButton : AppBarButton
    {
        private AutoSuggestBox textBox;

        public event EventHandler<IList<FeedItem>> SearchChanged;

        public SearchFeedButton(AutoSuggestBox textBox)
        {
            this.textBox = textBox;
            this.textBox.KeyDown += textBox_KeyDown;
            Icon = new SymbolIcon(Symbol.Zoom);
        }

        //void textBox_GotFocus(object sender, RoutedEventArgs e)
        //{
        //    Windows.UI.ViewManagement.InputPane.GetForCurrentView().TryShow();
        //}

        public bool Active {
            get { return textBox.Visibility == Visibility.Visible; }
            set { textBox.Visibility = value ? Visibility.Visible : Visibility.Collapsed; }
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






    class BackgroundTaskSample
    {
        public const string SampleBackgroundTaskEntryPoint = "ACastBackgroundTimerTask.BackgroundTimerTask";
        public const string SampleBackgroundTaskName = "BackgroundTimerTask";
        public static string SampleBackgroundTaskProgress = "";
        public static bool SampleBackgroundTaskRegistered = false;

        public const string SampleBackgroundTaskWithConditionName = "SampleBackgroundTaskWithCondition";
        public static string SampleBackgroundTaskWithConditionProgress = "";
        public static bool SampleBackgroundTaskWithConditionRegistered = false;

        public const string ServicingCompleteTaskEntryPoint = "Tasks.ServicingComplete";
        public const string ServicingCompleteTaskName = "ServicingCompleteTask";
        public static string ServicingCompleteTaskProgress = "";
        public static bool ServicingCompleteTaskRegistered = false;

        public const string TimeTriggeredTaskName = "TimeTriggeredTask";
        public static string TimeTriggeredTaskProgress = "";
        public static bool TimeTriggeredTaskRegistered = false;

        /// <summary>
        /// Register a background task with the specified taskEntryPoint, name, trigger,
        /// and condition (optional).
        /// </summary>
        /// <param name="taskEntryPoint">Task entry point for the background task.</param>
        /// <param name="name">A name for the background task.</param>
        /// <param name="trigger">The trigger for the background task.</param>
        /// <param name="condition">An optional conditional event that must be true for the task to fire.</param>
        public static async Task<BackgroundTaskRegistration> RegisterBackgroundTask(String taskEntryPoint, String name, IBackgroundTrigger trigger, IBackgroundCondition condition)
        {
            if (TaskRequiresBackgroundAccess(name))
            {
                await BackgroundExecutionManager.RequestAccessAsync();
            }

            var builder = new BackgroundTaskBuilder();

            builder.Name = name;
            builder.TaskEntryPoint = taskEntryPoint;
            builder.SetTrigger(trigger);

            if (condition != null)
            {
                builder.AddCondition(condition);

                //
                // If the condition changes while the background task is executing then it will
                // be canceled.
                //
                builder.CancelOnConditionLoss = true;
            }

            BackgroundTaskRegistration task = builder.Register();

            UpdateBackgroundTaskStatus(name, true);

            //
            // Remove previous completion status from local settings.
            //
            var settings = ApplicationData.Current.LocalSettings;
            settings.Values.Remove(name);

            return task;
        }

        /// <summary>
        /// Unregister background tasks with specified name.
        /// </summary>
        /// <param name="name">Name of the background task to unregister.</param>
        public static void UnregisterBackgroundTasks(string name)
        {
            //
            // Loop through all background tasks and unregister any with SampleBackgroundTaskName or
            // SampleBackgroundTaskWithConditionName.
            //
            foreach (var cur in BackgroundTaskRegistration.AllTasks)
            {
                if (cur.Value.Name == name)
                {
                    cur.Value.Unregister(true);
                }
            }

            UpdateBackgroundTaskStatus(name, false);
        }

        /// <summary>
        /// Store the registration status of a background task with a given name.
        /// </summary>
        /// <param name="name">Name of background task to store registration status for.</param>
        /// <param name="registered">TRUE if registered, FALSE if unregistered.</param>
        public static void UpdateBackgroundTaskStatus(String name, bool registered)
        {
            switch (name)
            {
                case SampleBackgroundTaskName:
                    SampleBackgroundTaskRegistered = registered;
                    break;
                case SampleBackgroundTaskWithConditionName:
                    SampleBackgroundTaskWithConditionRegistered = registered;
                    break;
                case ServicingCompleteTaskName:
                    ServicingCompleteTaskRegistered = registered;
                    break;
                case TimeTriggeredTaskName:
                    TimeTriggeredTaskRegistered = registered;
                    break;
            }
        }

        /// <summary>
        /// Get the registration / completion status of the background task with
        /// given name.
        /// </summary>
        /// <param name="name">Name of background task to retreive registration status.</param>
        public static String GetBackgroundTaskStatus(String name)
        {
            var registered = false;
            switch (name)
            {
                case SampleBackgroundTaskName:
                    registered = SampleBackgroundTaskRegistered;
                    break;
                case SampleBackgroundTaskWithConditionName:
                    registered = SampleBackgroundTaskWithConditionRegistered;
                    break;
                case ServicingCompleteTaskName:
                    registered = ServicingCompleteTaskRegistered;
                    break;
                case TimeTriggeredTaskName:
                    registered = TimeTriggeredTaskRegistered;
                    break;
            }

            var status = registered ? "Registered" : "Unregistered";

            var settings = ApplicationData.Current.LocalSettings;
            if (settings.Values.ContainsKey(name))
            {
                status += " - " + settings.Values[name].ToString();
            }

            return status;
        }

        /// <summary>
        /// Determine if task with given name requires background access.
        /// </summary>
        /// <param name="name">Name of background task to query background access requirement.</param>
        public static bool TaskRequiresBackgroundAccess(String name)
        {
#if WINDOWS_PHONE_APP
            return true;
#else
            if (name == TimeTriggeredTaskName)
            {
                return true;
            }
            else
            {
                return false;
            }
#endif
        }
    }

}
