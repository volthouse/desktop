using ACast.Database;
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
        private bool isBackgroundTaskRunning = false;

        public static Player Instance;

        public event EventHandler<MediaPlayerState> StateChanged;

        const int RPC_S_SERVER_UNAVAILABLE = -2147023174; // 0x800706BA

        public MediaPlayerState State
        {
            get { return BackgroundMediaPlayer.Current.CurrentState; }
        }

        public Player()
        {
            DebugService.Add("Player: created");
            //ApplicationSettingsHelper.SaveSettingsValue("Player", "Started");

            addMediaPlayerEventHandlers();

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
            if (isBackgroundTaskRunning)
            {
                // If yes, it's safe to reconnect to media play handlers
                addMediaPlayerEventHandlers();

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
            if (isBackgroundTaskRunning)
            {
                // Stop handling player events immediately
                removeMediaPlayerEventHandlers();

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
            string path = feedItem.Path + @"\" + feedItem.FileName;

            MessageService.SendMessageToBackground(new StartTrackMessage(
                new Uri(path), feedItem.Title, TimeSpan.Zero
            ));

            if (MediaPlayerState.Paused == currentPlayer.CurrentState)
            {
                currentPlayer.Play();
            }

        }        

        public void Play()
        {
            if (isBackgroundTaskRunning)
            {
                BackgroundMediaPlayer.Current.Play();
            }
        }

        public void Pause()
        {
            if (isBackgroundTaskRunning)
            {
                BackgroundMediaPlayer.Current.Pause();
            }
        }

        public void Resume()
        {
            if (isBackgroundTaskRunning)
            {
                MessageService.SendMessageToBackground(new ResumePlaybackMessage());
            }
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

        public void SetSleepTimer(int durationMin)
        {
            //if (isBackgroundTaskRunning)
            //{
                MessageService.SendMessageToBackground(new SetSleepTimerMessage(durationMin));
            //}
        }

        void backgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            DebugService.Add("Player: MessageReceivedFromBackground");

            BackgroundServiceIsAlive backgroundServiceIsAlive;
            if (MessageService.TryParseMessage(e.Data, out backgroundServiceIsAlive))
            {
                isBackgroundTaskRunning = true;
                DebugService.Add("Player: BackgroundAudioTask is alive");
            }


            BackgroundAudioTaskStartedMessage backgroundAudioTaskStartedMessage;
            if (MessageService.TryParseMessage(e.Data, out backgroundAudioTaskStartedMessage))
            {
                // StartBackgroundAudioTask is waiting for this signal to know when the task is up and running
                // and ready to receive messages
                DebugService.Add("Player: BackgroundAudioTask started");
                isBackgroundTaskRunning = true;
                return;
            }
        }

        /// <summary>
        /// MediaPlayer state changed event handlers. 
        /// Note that we can subscribe to events even if Media Player is playing media in background
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void mediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {
            if (StateChanged != null)
            {
                StateChanged(this, sender.CurrentState);
            }
        }

        /// <summary>
        /// Unsubscribes to MediaPlayer events. Should run only on suspend
        /// </summary>
        private void removeMediaPlayerEventHandlers()
        {
            try
            {
                BackgroundMediaPlayer.Current.CurrentStateChanged -= this.mediaPlayer_CurrentStateChanged;
                BackgroundMediaPlayer.MessageReceivedFromBackground -= backgroundMediaPlayer_MessageReceivedFromBackground;
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
        private void addMediaPlayerEventHandlers()
        {
            currentPlayer.CurrentStateChanged += this.mediaPlayer_CurrentStateChanged;

            try
            {
                BackgroundMediaPlayer.MessageReceivedFromBackground += backgroundMediaPlayer_MessageReceivedFromBackground;
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
        /// You should never cache the MediaPlayer and always call Current. It is possible
        /// for the background task to go away for several different reasons. When it does
        /// an RPC_S_SERVER_UNAVAILABLE error is thrown. We need to reset the foreground state
        /// and restart the background task.
        /// </summary>
        private MediaPlayer currentPlayer
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
