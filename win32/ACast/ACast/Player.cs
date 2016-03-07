using ACastShared;
using ACastShared.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Xaml;

namespace ACast
{
    public sealed class Player
    {
        private bool _isMyBackgroundTaskRunning = false;

        public static Player Instance;// = new Player();

        public event EventHandler<MediaPlayerState> StateChanged;

        const int RPC_S_SERVER_UNAVAILABLE = -2147023174; // 0x800706BA

        public CoreDispatcher Dispatcher;

        public MediaPlayerState State
        {
            get { return BackgroundMediaPlayer.Current.CurrentState; }
        }

        public Player()
        {
            Dispatcher = Window.Current.Dispatcher;

            DebugService.Add("Player: created");
            //ApplicationSettingsHelper.SaveSettingsValue("Player", "Started");

            AddMediaPlayerEventHandlers();

            MessageService.SendMessageToBackground(new IsBackgroundServiceAlive());
        }


        /// <summary>
        /// Sends message to background informing app has resumed
        /// Subscribe to MediaPlayer events
        /// </summary>
        public void ForegroundAppResuming()
        {
            DebugService.Add("Player: ForegroundAppResuming");

            //ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.AppState, AppState.Active.ToString());

            // Verify the task is running
            if (_isMyBackgroundTaskRunning)
            {
                // If yes, it's safe to reconnect to media play handlers
                AddMediaPlayerEventHandlers();

                // Send message to background task that app is resumed so it can start sending notifications again
                MessageService.SendMessageToBackground(new AppResumedMessage());
            }
        }

        /// <summary>
        /// Send message to Background process that app is to be suspended
        /// Stop clock and slider when suspending
        /// Unsubscribe handlers for MediaPlayer events
        /// </summary>
        public void ForegroundAppSuspending(Windows.ApplicationModel.SuspendingDeferral deferral)
        {
            // Only if the background task is already running would we do these, otherwise
            // it would trigger starting up the background task when trying to suspend.
            if (_isMyBackgroundTaskRunning)
            {
                // Stop handling player events immediately
                RemoveMediaPlayerEventHandlers();

                // Tell the background task the foreground is suspended
                MessageService.SendMessageToBackground(new AppSuspendedMessage());
            }

            // Persist that the foreground app is suspended
            //ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.AppState, AppState.Suspended.ToString());

            deferral.Complete();
        }

        public void Play(FeedItem feedItem)
        {
            DebugService.Add("Player: Play button pressed");
            string path = ApplicationData.Current.LocalFolder.Path + @"\" + feedItem.FileName;

            MessageService.SendMessageToBackground(new StartTrackMessage(new Uri(path)));

            if (MediaPlayerState.Paused == CurrentPlayer.CurrentState)
            {
                CurrentPlayer.Play();
            }

        }        

        public void Play()
        {
           // if (IsMyBackgroundTaskRunning)
           // {
                BackgroundMediaPlayer.Current.Play();
           // }
        }

        public void Pause()
        {
            // if (IsMyBackgroundTaskRunning)
            // {
            BackgroundMediaPlayer.Current.Pause();
            // }
        }

        public void Resume()
        {
           // if (IsMyBackgroundTaskRunning)
           // {
                MessageService.SendMessageToBackground(new ResumePlaybackMessage());
           // }
        }

        public double RelativePosition {
            get {
                double relativePos = BackgroundMediaPlayer.Current.Position.TotalMilliseconds /
                    BackgroundMediaPlayer.Current.NaturalDuration.TotalMilliseconds;
                if (relativePos > 0)
                {
                    return relativePos * 100;
                }

                return 0;
            }

            set
            {
                MediaPlayerState state = BackgroundMediaPlayer.Current.CurrentState;
                long relativePos = (long)(BackgroundMediaPlayer.Current.NaturalDuration.Ticks * Math.Min(100, value) / 100);
                BackgroundMediaPlayer.Current.Position = new TimeSpan(relativePos);
                // workaround, wenn Pause und position gesetzt wird läuft der Player wieder
                if (state == MediaPlayerState.Paused)
                {
                    BackgroundMediaPlayer.Current.Pause();
                }
            }
        }


        void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {

            ValueSet valueSet = e.Data;
            string s = "";

            foreach (var item in valueSet.Values)
            {
                s += item.ToString();
            }

            DebugService.Add("Player: MessageReceivedFromBackground:\r\n" + s);

            BackgroundServiceIsAlive backgroundServiceIsAlive;
            if (MessageService.TryParseMessage(e.Data, out backgroundServiceIsAlive))
            {
                _isMyBackgroundTaskRunning = true;
                DebugService.Add("Player: BackgroundAudioTask is alive");
            }


            BackgroundAudioTaskStartedMessage backgroundAudioTaskStartedMessage;
            if (MessageService.TryParseMessage(e.Data, out backgroundAudioTaskStartedMessage))
            {
                // StartBackgroundAudioTask is waiting for this signal to know when the task is up and running
                // and ready to receive messages
                DebugService.Add("Player: BackgroundAudioTask started");
                _isMyBackgroundTaskRunning = true;
                return;
            }
        }

        /// <summary>
        /// Unsubscribes to MediaPlayer events. Should run only on suspend
        /// </summary>
        private void RemoveMediaPlayerEventHandlers()
        {
            try
            {
                BackgroundMediaPlayer.Current.CurrentStateChanged -= this.MediaPlayer_CurrentStateChanged;
                BackgroundMediaPlayer.MessageReceivedFromBackground -= BackgroundMediaPlayer_MessageReceivedFromBackground;
            }
            catch (Exception ex)
            {
                if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
                {
                    // do nothing
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Subscribes to MediaPlayer events
        /// </summary>
        private void AddMediaPlayerEventHandlers()
        {
            CurrentPlayer.CurrentStateChanged += this.MediaPlayer_CurrentStateChanged;

            try
            {
                BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
            }
            catch (Exception ex)
            {
                DebugService.Add("Player: AddMediaPlayerEventHandlers:" + ex.Message);
                if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
                {
                    // Internally MessageReceivedFromBackground calls Current which can throw RPC_S_SERVER_UNAVAILABLE
                    //ResetAfterLostBackground();
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// MediaPlayer state changed event handlers. 
        /// Note that we can subscribe to events even if Media Player is playing media in background
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /*async*/
        void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            if (StateChanged != null)
            {
                StateChanged(this, sender.CurrentState);
            }
        }

        /// <summary>
        /// You should never cache the MediaPlayer and always call Current. It is possible
        /// for the background task to go away for several different reasons. When it does
        /// an RPC_S_SERVER_UNAVAILABLE error is thrown. We need to reset the foreground state
        /// and restart the background task.
        /// </summary>
        private MediaPlayer CurrentPlayer
        {
            get
            {
                MediaPlayer mp = null;
                int retryCount = 2;

                while (mp == null && --retryCount >= 0)
                {
                    try
                    {
                        mp = BackgroundMediaPlayer.Current;
                    }
                    catch (Exception ex)
                    {
                        if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
                        {
                            // The foreground app uses RPC to communicate with the background process.
                            // If the background process crashes or is killed for any reason RPC_S_SERVER_UNAVAILABLE
                            // is returned when calling Current. We must restart the task, the while loop will retry to set mp.
                            //ResetAfterLostBackground();
                            //StartBackgroundAudioTask("");
                        }
                        else
                        {
                            throw;
                        }
                    }
                }

                if (mp == null)
                {
                    throw new Exception("Failed to get a MediaPlayer instance.");
                }

                return mp;
            }
        }
    }
}
