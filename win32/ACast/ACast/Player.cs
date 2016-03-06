﻿using ACastShared;
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
        private AutoResetEvent backgroundAudioTaskStarted;

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
            backgroundAudioTaskStarted = new AutoResetEvent(false);
            Dispatcher = Window.Current.Dispatcher;

            DebugService.Add("Player: created");
            ApplicationSettingsHelper.SaveSettingsValue("Player", "Started");

            AddMediaPlayerEventHandlers();

            MessageService.SendMessageToBackground(new IsBackgroundServiceAlive());
        }


        private void ResetAfterLostBackground()
        {
            BackgroundMediaPlayer.Shutdown();
            _isMyBackgroundTaskRunning = false;
            backgroundAudioTaskStarted.Reset();
            //prevButton.IsEnabled = true;
            //nextButton.IsEnabled = true;
            ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.BackgroundTaskState, BackgroundTaskState.Unknown.ToString());
            //playButton.Content = "| |";

            try
            {
                BackgroundMediaPlayer.MessageReceivedFromBackground += BackgroundMediaPlayer_MessageReceivedFromBackground;
            }
            catch (Exception ex)
            {
                if (ex.HResult == RPC_S_SERVER_UNAVAILABLE)
                {
                    throw new Exception("Failed to get a MediaPlayer instance.");
                }
                else
                {
                    throw;
                }
            }
        }

        /// <summary>
        /// Sends message to background informing app has resumed
        /// Subscribe to MediaPlayer events
        /// </summary>
        public void ForegroundAppResuming()
        {
            DebugService.Add("Player: ForegroundAppResuming");

            ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.AppState, AppState.Active.ToString());

            // Verify the task is running
            if (IsMyBackgroundTaskRunning)
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
            if (IsMyBackgroundTaskRunning)
            {
                // Stop handling player events immediately
                RemoveMediaPlayerEventHandlers();

                // Tell the background task the foreground is suspended
                MessageService.SendMessageToBackground(new AppSuspendedMessage());
            }

            // Persist that the foreground app is suspended
            ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.AppState, AppState.Suspended.ToString());

            deferral.Complete();
        }

        /// <summary>
        /// Gets the information about background task is running or not by reading the setting saved by background task
        /// </summary>
        public bool IsMyBackgroundTaskRunning
        {
            get
            {
                MessageService.SendMessageToBackground(new IsBackgroundServiceAlive());

                if (_isMyBackgroundTaskRunning)
                {
                    DebugService.Add("Player: IsMyBackgroundTaskRunning: running");
                } else
                {
                    DebugService.Add("Player: IsMyBackgroundTaskRunning: not running");
                }
                
                if (_isMyBackgroundTaskRunning)
                    return true;

                string value = ApplicationSettingsHelper.ReadResetSettingsValue(ApplicationSettingsConstants.BackgroundTaskState) as string;
                if(value == null) {
                    DebugService.Add("Player: IsMyBackgroundTaskRunning: value = null");
                } else
                {
                    DebugService.Add("Player: IsMyBackgroundTaskRunning: value = " + value.ToString());
                }

                
                if (value == null)
                {
                    return false;
                }
                else
                {
                    try
                    {
                        _isMyBackgroundTaskRunning = EnumHelper.Parse<BackgroundTaskState>(value) == BackgroundTaskState.Running;
                    }
                    catch (ArgumentException)
                    {
                        _isMyBackgroundTaskRunning = false;
                    }
                    return _isMyBackgroundTaskRunning;
                }
            }
        }



        public void Play(FeedItem feedItem)
        {
            DebugService.Add("Player: Play button pressed");
            string path = ApplicationData.Current.LocalFolder.Path + @"\" + feedItem.FileName;

            // Start the background task if it wasn't running
            if (!IsMyBackgroundTaskRunning /*|| MediaPlayerState.Closed == CurrentPlayer.CurrentState*/)
            {
                // First update the persisted start track
                ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.TrackId, path);
                ApplicationSettingsHelper.SaveSettingsValue(ApplicationSettingsConstants.Position, new TimeSpan().ToString());

                // Start task
                StartBackgroundAudioTask(path);
            }
            else
            {
                // Switch to the selected track
                MessageService.SendMessageToBackground(new StartTrackMessage(new Uri(path)));
            }

            if (MediaPlayerState.Paused == CurrentPlayer.CurrentState)
            {
                CurrentPlayer.Play();
            }

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
                BackgroundMediaPlayer.Current.Play();
           // }
        }

        public void ResumeEx()
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


        private void StartBackgroundAudioTask(string filePath)
        {
            DebugService.Add("Player: StartBackgroundAudioTask");
            AddMediaPlayerEventHandlers();

            var startResult = Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                bool result = backgroundAudioTaskStarted.WaitOne(10000);
                //Send message to initiate playback
                if (result == true)
                {
                    DebugService.Add("Player: StartBackgroundAudioTask: Ok");
                    MessageService.SendMessageToBackground(new StartTrackMessage(new Uri(filePath)));
                }
                else
                {
                    DebugService.Add("Player: StartBackgroundAudioTask: Failed");
                    throw new Exception("Background Audio Task didn't start in expected time");
                }
            });
            startResult.Completed = new AsyncActionCompletedHandler(BackgroundTaskInitializationCompleted);
        }

        private void BackgroundTaskInitializationCompleted(IAsyncAction action, AsyncStatus status)
        {
            if (status == AsyncStatus.Completed)
            {
                DebugService.Add("Player: Background Audio Task initialized");
            }
            else if (status == AsyncStatus.Error)
            {
                DebugService.Add("Player: Background Audio Task could not initialized due to an error ::" + action.ErrorCode.ToString());
            }
        }

        async void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
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
            }

                TrackChangedMessage trackChangedMessage;
            if (MessageService.TryParseMessage(e.Data, out trackChangedMessage))
            {
                // When foreground app is active change track based on background message
                await Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    // If playback stopped then clear the UI
                    if (trackChangedMessage.TrackId == null)
                    {
                        //playlistView.SelectedIndex = -1;
                        //albumArt.Source = null;
                        //txtCurrentTrack.Text = string.Empty;
                        //prevButton.IsEnabled = false;
                        //nextButton.IsEnabled = false;
                        return;
                    }

                    //var songIndex = playlistView.GetSongIndexById(trackChangedMessage.TrackId);
                    //var song = playlistView.Songs[songIndex];

                    //// Update list UI
                    //playlistView.SelectedIndex = songIndex;

                    //// Update the album art
                    //albumArt.Source = albumArtCache[song.AlbumArtUri.ToString()];

                    //// Update song title
                    //txtCurrentTrack.Text = song.Title;

                    //// Ensure track buttons are re-enabled since they are disabled when pressed
                    //prevButton.IsEnabled = true;
                    //nextButton.IsEnabled = true;
                });
                return;
            }

            BackgroundAudioTaskStartedMessage backgroundAudioTaskStartedMessage;
            if (MessageService.TryParseMessage(e.Data, out backgroundAudioTaskStartedMessage))
            {
                // StartBackgroundAudioTask is waiting for this signal to know when the task is up and running
                // and ready to receive messages
                DebugService.Add("Player: BackgroundAudioTask started");
                backgroundAudioTaskStarted.Set();
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
                    ResetAfterLostBackground();
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
                            ResetAfterLostBackground();
                            StartBackgroundAudioTask("");
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
