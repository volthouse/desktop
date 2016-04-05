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
using ACast.Database;

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

        private ViewManager viewManager;
        private FeedDialog feedDialog;
        private FeedItemsDialog feedItemsDialog;


        public static MainPage Instance;

        public MainPage()
        {
            Player.Instance = new Player();

            Instance = this;

            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            context = SynchronizationContext.Current;

            viewManager = new ViewManager();
            feedDialog = new FeedDialog(context);
            feedItemsDialog = new FeedItemsDialog(context);

            viewManager.Pivot = pivot;
            viewManager.Dialogs.Add(feedDialog);
            viewManager.Dialogs.Add(feedItemsDialog);

            feedDialog.CommandBar = commandBar;
            feedDialog.ViewManager = viewManager;
            feedDialog.View = feedListView;
            feedDialog.SearchTextBox = serachFeedTextBox;

            feedItemsDialog.CommandBar = commandBar;
            feedItemsDialog.ViewManager = viewManager;
            feedItemsDialog.View = feedItemsListView;
            feedItemsDialog.SearchTextBox = serachFeedDetailsTextBox;
            feedItemsDialog.PickerButton = pickerButton;
            feedItemsDialog.PlayButton = playButton;

            SQLiteDb.Create();

            viewManager.SwitchTo(PivotView.Feeds);

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

            DebugService.Instance.Serialize();

#endif           
        }

        private void app_Resuming(object sender, object e)
        {
            
            Player.Instance.ForegroundAppResuming();
        }

        private void app_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
            Player.Instance.ForegroundAppSuspending(e.SuspendingOperation.GetDeferral());
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

    public class AppBarButtonBase : AppBarButton
    {

        public AppBarButtonBase()
        {
            Debug.WriteLine("Add button");
        }

        public void Show(bool visible)
        {
            Visibility = visible ? Visibility.Visible : Visibility.Collapsed;
        }

        public void Hide(bool visible)
        {
            Visibility = visible ? Visibility.Collapsed : Visibility.Visible;
        }
    }

    public class AddButton : AppBarButtonBase
    {
        public AddButton()
        {
            Icon = new SymbolIcon(Symbol.Add);
        }
    }

    public class RemoveButton : AppBarButtonBase
    {
        public RemoveButton()
        {
            Icon = new SymbolIcon(Symbol.Delete);
        }
    }

    public class RefreshButton : AppBarButtonBase
    {
        public RefreshButton()
        {
            Icon = new SymbolIcon(Symbol.Refresh);
        }
    }

    public class MultiSelectButton : AppBarButtonBase
    {
        public MultiSelectButton()
        {
            Icon = new SymbolIcon(Symbol.Bullets);
        }
    }

    public class CancelButton : AppBarButtonBase
    {
        public CancelButton()
        {
            Icon = new SymbolIcon(Symbol.Cancel);
        }
    }

    public class SearchButton : AppBarButtonBase
    {
        public SearchButton()
        {
            Icon = new SymbolIcon(Symbol.Zoom);
        }
    }

}
