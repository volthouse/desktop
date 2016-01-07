using ACastBackgroundAudioTask;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace ACast
{
    public class Player
    {
        private AutoResetEvent SererInitialized;

        public static Player Instance = new Player();

        public Player()
        {
            SererInitialized = new AutoResetEvent(false);
        }

        private void RemoveMediaPlayerEventHandlers()
        {
            BackgroundMediaPlayer.Current.CurrentStateChanged -= this.MediaPlayer_CurrentStateChanged;
            BackgroundMediaPlayer.MessageReceivedFromBackground -= this.BackgroundMediaPlayer_MessageReceivedFromBackground;
        }

        /// <summary>
        /// Subscribes to MediaPlayer events
        /// </summary>
        private void AddMediaPlayerEventHandlers()
        {
            BackgroundMediaPlayer.Current.CurrentStateChanged += this.MediaPlayer_CurrentStateChanged;
            BackgroundMediaPlayer.MessageReceivedFromBackground += this.BackgroundMediaPlayer_MessageReceivedFromBackground;
        }

        public void Play(FeedItem feedItem)
        {

            BackgroundMediaPlayer.Current.CurrentStateChanged += this.MediaPlayer_CurrentStateChanged;
            BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;

            
            var backgroundtaskinitializationresult = Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                bool result = SererInitialized.WaitOne(2000);
                //Send message to initiate playback
                if (result == true)
                {
                    string path = ApplicationData.Current.LocalFolder.Path + @"\" + feedItem.FileName;

                    var message = new ValueSet();
                    message.Add(Constants.AddTrack, path);
                    BackgroundMediaPlayer.SendMessageToBackground(message);

                    message = new ValueSet();
                    message.Add(Constants.StartPlayback, "0");
                    BackgroundMediaPlayer.SendMessageToBackground(message);
                }
                else
                {
                    throw new Exception("Background Audio Task didn't start in expected time");
                }
            }
            );

            
            //BackgroundMediaPlayer.Current.SetUriSource(new Uri(path));
            //BackgroundMediaPlayer.Current.Play();


        }

        private async void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            switch (sender.CurrentState)
            {
                case MediaPlayerState.Playing:
                    //await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    //{
                    //    //playButton.Content = "| |";     // Change to pause button
                    //    //prevButton.IsEnabled = true;
                    //    //nextButton.IsEnabled = true;
                    //}
                    //    );

                    break;
                case MediaPlayerState.Paused:
                    //await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                    //{
                    //    //playButton.Content = ">";     // Change to play button
                    //}
                    //);

                    break;
            }
        }

        private async void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            foreach (string key in e.Data.Keys)
            {
                switch (key)
                {
                    case Constants.Trackchanged:
                        //When foreground app is active change track based on background message
                        await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        {
                            //txtCurrentTrack.Text = (string)e.Data[key];
                        }
                        );
                        break;
                    case Constants.BackgroundTaskStarted:
                        //Wait for Background Task to be initialized before starting playback
                        //Debug.WriteLine("Background Task started");
                        SererInitialized.Set();
                        break;
                }
            }
        }

        private void BackgroundMediaPlayer_MessageReceivedFromForeground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            //throw new NotImplementedException();
        }

        void Current_MediaFailed(MediaPlayer sender, MediaPlayerFailedEventArgs args)
        {

        }

        void Current_CurrentStateChanged(MediaPlayer sender, object args)
        {
            var x = BackgroundMediaPlayer.Current.CurrentState;
        }
    }
}
