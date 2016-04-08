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
using ACast.UI;

// The Blank Page item template is documented at http://go.microsoft.com/fwlink/?LinkId=391641

namespace ACast
{
    /// <summary>
    /// An empty page that can be used on its own or navigated to within a Frame.
    /// </summary>
    public sealed partial class MainPage : Page
    {
        private SynchronizationContext context;

        private CommandBarManager commandBarManager;
        private ViewManager viewManager;
        
        public static MainPage Instance;

        public MainPage()
        {
            Player.Instance = new Player();

            Instance = this;

            this.InitializeComponent();

            this.NavigationCacheMode = NavigationCacheMode.Required;

            context = SynchronizationContext.Current;

            viewManager = new ViewManager();
            viewManager.Pivot = pivot;

            commandBarManager = new CommandBarManager();
            commandBarManager.CommandBar = commandBar;
            commandBarManager.ButtonClick += feedViewControl.CommandBarButtonClick;

            viewManager.ActiveViewChanged += feedViewControl.ActiveViewChanged;
            viewManager.ActiveViewChanged += feedItemsViewControl.ActiveViewChanged;

            feedViewControl.EnableButtons += commandBarManager.EnableButtons;
            feedViewControl.ActivateView += viewManager.ActivateView;

            feedItemsViewControl.EnableButtons += commandBarManager.EnableButtons;
            feedItemsViewControl.ActivateView += viewManager.ActivateView;
            feedItemsViewControl.Play += Player.Instance.Play;

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
}
