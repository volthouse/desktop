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
            timer.Tick += Timer_Tick;

            Player.Instance.StateChanged += Instance_StateChanged;

            playButton.Click += playButton_Click;
            forwardButton.Click += ForwardButton_Click;
            rewardButton.Click += RewardButton_Click;

            posSlider.PointerEntered += PosSlider_PointerEntered;
            posSlider.PointerExited += PosSlider_PointerExited;

            smtc = SystemMediaTransportControls.GetForCurrentView();

            
           // smtc.PlaybackStatus
        }

        public void Activate()
        {
            Instance_StateChanged(null, Player.Instance.State);
        }

        private void PosSlider_PointerEntered(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            timer.Stop();
        }

        private void PosSlider_PointerExited(object sender, Windows.UI.Xaml.Input.PointerRoutedEventArgs e)
        {
            Player.Instance.RelativePosition = posSlider.Value;
            timer.Start();
        }

        private void RewardButton_Click(object sender, RoutedEventArgs e)
        {
            posSlider.Value -= 10;
            Player.Instance.RelativePosition = posSlider.Value;
        }

        private void ForwardButton_Click(object sender, RoutedEventArgs e)
        {
            posSlider.Value += 10;
            Player.Instance.RelativePosition = posSlider.Value;
        }

        private void Timer_Tick(object sender, object e)
        {
            posSlider.Value = Player.Instance.RelativePosition;
        }

        private void Instance_StateChanged(object sender, MediaPlayerState e)
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
            if (Player.Instance.State == MediaPlayerState.Playing)
            {
                Player.Instance.Pause();                
            }
            else if (Player.Instance.State == MediaPlayerState.Paused)
            {
                Player.Instance.Resume();
                
            }
        }

    }
}
