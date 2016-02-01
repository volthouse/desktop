using BackgroundAudioShared;
using BackgroundAudioShared.Messages;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Foundation.Collections;
using Windows.Media.Playback;
using Windows.Storage;
using Windows.UI.Core;
using Windows.UI.Popups;
using Windows.UI.Xaml;

namespace ACast
{
    public sealed class Player
    {
        private AutoResetEvent SererInitialized;
        private bool isMyBackgroundTaskRunning = false;
        private bool _isMyBackgroundTaskRunning = false;
        private AutoResetEvent backgroundAudioTaskStarted;

        public static Player Instance = new Player();

        public event EventHandler<MediaPlayerState> StateChanged;

        const int RPC_S_SERVER_UNAVAILABLE = -2147023174; // 0x800706BA

        public MediaPlayerState State {
            get { return BackgroundMediaPlayer.Current.CurrentState; }
        }

        public Player()
        {
            SererInitialized = new AutoResetEvent(false);
            backgroundAudioTaskStarted = new AutoResetEvent(false);

            App.Current.Suspending += ForegroundApp_Suspending;
            App.Current.Resuming += ForegroundApp_Resuming;

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

        #region Foreground App Lifecycle Handlers
        /// <summary>
        /// Sends message to background informing app has resumed
        /// Subscribe to MediaPlayer events
        /// </summary>
        public void ForegroundApp_Resuming(object sender, object e)
        {
           

            ApplicationSettingsHelper.SaveSettingsValue(Constants.AppState, Constants.ForegroundAppActive);

            // Verify if the task was running before
            if (IsMyBackgroundTaskRunning)
            {
                //if yes, reconnect to media play handlers
                AddMediaPlayerEventHandlers();

                //send message to background task that app is resumed, so it can start sending notifications
                ValueSet messageDictionary = new ValueSet();
                messageDictionary.Add(Constants.AppResumed, DateTime.Now.ToString());
                BackgroundMediaPlayer.SendMessageToBackground(messageDictionary);                              

                if (StateChanged != null)
                {
                    StateChanged(this, BackgroundMediaPlayer.Current.CurrentState);
                }               
            }
            else
            {
                if (StateChanged != null)
                {
                    StateChanged(this, MediaPlayerState.Closed);
                }  
            }

            //if(isMyBackgroundTaskRunning)
            //{
            //    var dialog = new MessageDialog("resume: task is running");
            //    dialog.ShowAsync();
            //} else
            //{
            //    var dialog = new MessageDialog("resume: task not running");
            //    dialog.ShowAsync();
            //}
            

        }

        /// <summary>
        /// Send message to Background process that app is to be suspended
        /// Stop clock and slider when suspending
        /// Unsubscribe handlers for MediaPlayer events
        /// </summary>
        void ForegroundApp_Suspending(object sender, Windows.ApplicationModel.SuspendingEventArgs e)
        {
           

            var deferral = e.SuspendingOperation.GetDeferral();
            ValueSet messageDictionary = new ValueSet();
            messageDictionary.Add(Constants.AppSuspended, DateTime.Now.ToString());
            BackgroundMediaPlayer.SendMessageToBackground(messageDictionary);
            RemoveMediaPlayerEventHandlers();
            ApplicationSettingsHelper.SaveSettingsValue(Constants.AppState, Constants.ForegroundAppSuspended);
            deferral.Complete();
        }
        #endregion

        /// <summary>
        /// Gets the information about background task is running or not by reading the setting saved by background task
        /// </summary>
        public bool IsMyBackgroundTaskRunning
        {
            get
            {
                if (isMyBackgroundTaskRunning)
                    return true;

                object value = ApplicationSettingsHelper.ReadResetSettingsValue(Constants.BackgroundTaskState);
                if (value == null)
                {
                    return false;
                }
                else
                {                   
                    isMyBackgroundTaskRunning = ((String)value).Equals(Constants.BackgroundTaskRunning);
                    return isMyBackgroundTaskRunning;
                }
            }
        }

        /// <summary>
        /// Read current track information from application settings
        /// </summary>
        public string CurrentTrack
        {
            get
            {
                object value = ApplicationSettingsHelper.ReadResetSettingsValue(Constants.CurrentTrack);
                if (value != null)
                {
                    return (String)value;
                }
                else
                    return String.Empty;
            }
        }

        public void Test()
        {

        }

        public void Play(FeedItem feedItem)
        {
            Debug.WriteLine("Play button pressed from App");
            string path = ApplicationData.Current.LocalFolder.Path + @"\" + feedItem.FileName;
            StartBackgroundAudioTask(path);            
        }

        public void Pause()
        {
            if (IsMyBackgroundTaskRunning)
            {
                BackgroundMediaPlayer.Current.Pause();
            }
        }

        public void Resume()
        {
            if (IsMyBackgroundTaskRunning)
            {
                BackgroundMediaPlayer.Current.Play();
            }
        }

        #region Media Playback Helper methods
        /// <summary>
        /// Unsubscribes to MediaPlayer events. Should run only on suspend
        /// </summary>
        //private void RemoveMediaPlayerEventHandlers()
        //{
        //    BackgroundMediaPlayer.Current.CurrentStateChanged -= this.MediaPlayer_CurrentStateChanged;
        //    BackgroundMediaPlayer.MessageReceivedFromBackground -= this.BackgroundMediaPlayer_MessageReceivedFromBackground;
        //}

        /// <summary>
        /// Subscribes to MediaPlayer events
        /// </summary>
        //private void AddMediaPlayerEventHandlers()
        //{
        //    BackgroundMediaPlayer.Current.CurrentStateChanged += this.MediaPlayer_CurrentStateChanged;
        //    BackgroundMediaPlayer.MessageReceivedFromBackground += this.BackgroundMediaPlayer_MessageReceivedFromBackground;
        //}

        /// <summary>
        /// Initialize Background Media Player Handlers and starts playback
        /// </summary>
        //private void StartBackgroundAudioTask(string filePath)
        //{
        //    if (isMyBackgroundTaskRunning)
        //    {
        //        var message = new ValueSet();
        //        message.Add(Constants.AddTrack, filePath);
        //        BackgroundMediaPlayer.SendMessageToBackground(message);

        //        message = new ValueSet();
        //        message.Add(Constants.StartPlayback, "0");
        //        BackgroundMediaPlayer.SendMessageToBackground(message);
        //    }
        //    else
        //    {
        //        AddMediaPlayerEventHandlers();
        //        var backgroundtaskinitializationresult = Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        //        {
        //            ValueSet message = new ValueSet();
        //            message.Add(Constants.IsRunning, "");
        //            BackgroundMediaPlayer.SendMessageToBackground(message);

        //            bool result = SererInitialized.WaitOne(10000);
        //            //Send message to initiate playback
        //            if (result == true)
        //            {
        //                message = new ValueSet();
        //                message.Add(Constants.AddTrack, filePath);
        //                BackgroundMediaPlayer.SendMessageToBackground(message);

        //                //message = new ValueSet();
        //                //message.Add(Constants.StartPlayback, "0");
        //                //BackgroundMediaPlayer.SendMessageToBackground(message);
        //            }
        //            else
        //            {
        //                throw new Exception("Background Audio Task didn't start in expected time");
        //            }
        //        }
        //        );
        //        backgroundtaskinitializationresult.Completed = new AsyncActionCompletedHandler(BackgroundTaskInitializationCompleted);
        //    }
        //}

        //private void BackgroundTaskInitializationCompleted(IAsyncAction action, AsyncStatus status)
        //{
        //    if (status == AsyncStatus.Completed)
        //    {
        //        Debug.WriteLine("Background Audio Task initialized");
        //    }
        //    else if (status == AsyncStatus.Error)
        //    {
        //        Debug.WriteLine("Background Audio Task could not initialized due to an error ::" + action.ErrorCode.ToString());
        //    }
        //}

        /// <summary>
        /// Initialize Background Media Player Handlers and starts playback
        /// </summary>
        private void StartBackgroundAudioTask(string filePath)
        {
            AddMediaPlayerEventHandlers();

            var startResult = Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
            {
                bool result = backgroundAudioTaskStarted.WaitOne(10000);
                //Send message to initiate playback
                if (result == true)
                {
                    var list = new List<SongModel>();
                    list.Add(new SongModel() { Title = "Test", MediaUri = new Uri(filePath) });
                    MessageService.SendMessageToBackground(new UpdatePlaylistMessage(list));
                    MessageService.SendMessageToBackground(new StartPlaybackMessage());
                }
                else
                {
                    throw new Exception("Background Audio Task didn't start in expected time");
                }
            });
            startResult.Completed = new AsyncActionCompletedHandler(BackgroundTaskInitializationCompleted);
        }

        private void BackgroundTaskInitializationCompleted(IAsyncAction action, AsyncStatus status)
        {
            if (status == AsyncStatus.Completed)
            {
                Debug.WriteLine("Background Audio Task initialized");
            }
            else if (status == AsyncStatus.Error)
            {
                Debug.WriteLine("Background Audio Task could not initialized due to an error ::" + action.ErrorCode.ToString());
            }
        }

        async void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            TrackChangedMessage trackChangedMessage;
            if (MessageService.TryParseMessage(e.Data, out trackChangedMessage))
            {
                // When foreground app is active change track based on background message
                await Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
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
                Debug.WriteLine("BackgroundAudioTask started");
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
        #endregion

        #region Background MediaPlayer Event handlers
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
        /// <summary>
        /// This event fired when a message is recieved from Background Process
        /// </summary>
        //void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        //{
        //    foreach (string key in e.Data.Keys)
        //    {
        //        switch (key)
        //        {
        //            case Constants.Trackchanged:
        //                //When foreground app is active change track based on background message
        //                //await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
        //                //{
        //                //    txtCurrentTrack.Text = (string)e.Data[key];
        //                //}
        //                //);
        //                break;
        //            case Constants.BackgroundTaskStarted:
        //                //Wait for Background Task to be initialized before starting playback
        //                Debug.WriteLine("Background Task started");
        //                SererInitialized.Set();
        //                break;
        //        }
        //    }
        //}

        #endregion
    }
}
