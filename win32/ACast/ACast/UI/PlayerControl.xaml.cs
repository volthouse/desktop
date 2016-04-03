using ACastShared;
using System;
using System.Threading;
using Windows.ApplicationModel.Background;
using Windows.Data.Html;
using Windows.Media;
using Windows.Media.Playback;
using Windows.UI.Xaml;
using Windows.UI.Xaml.Controls;
using Windows.UI.Xaml.Controls.Primitives;

// The User Control item template is documented at http://go.microsoft.com/fwlink/?LinkId=234236

namespace ACast
{
    public sealed partial class PlayerControl : UserControl
    {
        private SynchronizationContext context;
        private DispatcherTimer timer;
        private FeedItemObsolet currentFeedItem;

        public PlayerControl()
        {
            this.InitializeComponent();

            context = SynchronizationContext.Current;

            timer = new DispatcherTimer();
            timer.Interval = TimeSpan.FromMilliseconds(1000);
            timer.Tick += timer_Tick;

            Player.Instance.StateChanged += player_StateChanged;

            playButton.Click += playButton_Click;
            forwardButton.Click += forwardButton_Click;
            rewardButton.Click += rewardButton_Click;

            posSlider.PointerEntered += posSlider_PointerEntered;
            posSlider.PointerExited += posSlider_PointerExited;

            foreach (MenuFlyoutItem item in sleepTimerFlyout.Items)
            {
                item.Click += sleepItemClick;
            }

        }

        public void Activate()
        {
            player_StateChanged(null, Player.Instance.State);
            //if(currentFeedItem == null)
            //{
            //    string lastFeedId = ApplicationSettingsHelper.ReadSettingsValue<string>(ApplicationSettingsConstants.LastFeedId);
            //    string lastFeedItemId = ApplicationSettingsHelper.ReadSettingsValue<string>(ApplicationSettingsConstants.LastFeedItemId);
            //    if(!string.IsNullOrEmpty(lastFeedId))
            //    {
            //        FeedManager.Instance.ActivateFeedAsync(lastFeedId,
            //            context.Post(new SendOrPostCallback((o) =>
            //            {
            //                Show(null);
            //            }), null)
            //        );
            //    }

            //}
        }

        public void Play(FeedItemObsolet feedItem)
        {
            currentFeedItem = feedItem;
            textBox.Text = HtmlUtilities.ConvertToText(currentFeedItem.Summary);
            updateApplicationSettings(feedItem);

            Player.Instance.Play(currentFeedItem);
        }

        public void Show(FeedItemObsolet feedItem)
        {
            currentFeedItem = feedItem;
            textBox.Text = HtmlUtilities.ConvertToText(currentFeedItem.Summary);
            updateApplicationSettings(feedItem);
        }

        private void updateApplicationSettings(FeedItemObsolet item)
        {
            ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.LastFeedId, item.ParentId);
            ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.LastFeedItemId, item.Id);
        }

        private void sleepItemClick(object sender, RoutedEventArgs e)
        {
            SleepTimerItem item = sender as SleepTimerItem;
            if(item != null)
            {
                Player.Instance.SetSleepTimer(item.Duration);

                //DebugService.Add("Start Background Timer");
                //var task = BackgroundTaskSample.RegisterBackgroundTask(BackgroundTaskSample.SampleBackgroundTaskEntryPoint,
                //                                                   BackgroundTaskSample.TimeTriggeredTaskName,
                //                                                   new TimeTrigger(30, true),
                //                                                   null);
                //await task;
            }
        }

        private void posSlider_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            timer.Stop();
        }

        private void posSlider_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Player.Instance.RelativePosition = posSlider.Value;
            timer.Start();
        }

        private void rewardButton_Click(object sender, RoutedEventArgs e)
        {
            posSlider.Value -= 2;
            Player.Instance.RelativePosition = posSlider.Value;
        }

        private void forwardButton_Click(object sender, RoutedEventArgs e)
        {
            posSlider.Value += 2;
            Player.Instance.RelativePosition = posSlider.Value;
        }

        private void timer_Tick(object sender, object e)
        {
            posSlider.Value = Player.Instance.RelativePosition;
        }

        private void player_StateChanged(object sender, MediaPlayerState e)
        {
            context.Post(new SendOrPostCallback((o) =>
            {
                switch (e)
                {
                    case MediaPlayerState.Opening:
                        break;
                    case MediaPlayerState.Buffering:
                        break;
                    case MediaPlayerState.Playing:
                        timer.Start();
                        playButton.Icon = new SymbolIcon(Symbol.Pause);
                        break;
                    
                    case MediaPlayerState.Paused:
                    case MediaPlayerState.Stopped:
                    case MediaPlayerState.Closed:
                        timer.Stop();
                        playButton.Icon = new SymbolIcon(Symbol.Play);
                        break;
                    default:
                        break;
                }

                posSlider.Value = Player.Instance.RelativePosition;

            }), null);
        }

        void playButton_Click(object sender, RoutedEventArgs e)
        {

            switch (Player.Instance.State)
            {
                case MediaPlayerState.Buffering:
                    break;
                case MediaPlayerState.Closed:
                    if(currentFeedItem != null)
                        Player.Instance.Play(currentFeedItem);
                    else
                        Player.Instance.Resume();
                    break;
                case MediaPlayerState.Opening:
                    break;
                case MediaPlayerState.Paused:
                    Player.Instance.Play();
                    break;
                case MediaPlayerState.Playing:
                    Player.Instance.Pause();
                    break;
                case MediaPlayerState.Stopped:
                    break;
                default:
                    break;
            }

        }
    }

    public class SleepTimerItem : MenuFlyoutItem
    {
        public SleepTimerItem() { 
        }

        public int Duration { get; set; }
    }
}
