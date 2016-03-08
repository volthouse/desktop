using System;
using System.Threading;
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
        private SystemMediaTransportControls smtc;

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

            sleepTimerComboBox.SelectionChanged += sleepTimerComboBox_SelectionChanged;
        }        

        public void Activate()
        {
            var x = BackgroundMediaPlayer.Current.CurrentState;

            textBox.Text = x.ToString();

            player_StateChanged(null, Player.Instance.State);
        }

        void sleepTimerComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int[] t = { 5 * 60000, 15 * 60000, 30 * 60000, 60 * 60000 };

            Player.Instance.SetSleepTimer(t[sleepTimerComboBox.SelectedIndex]);
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
            posSlider.Value -= 5;
            Player.Instance.RelativePosition = posSlider.Value;
        }

        private void forwardButton_Click(object sender, RoutedEventArgs e)
        {
            posSlider.Value += 5;
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
}
