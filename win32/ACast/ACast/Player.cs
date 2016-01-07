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
using Windows.UI.Xaml;

namespace ACast
{
    public sealed class Player
    {
        private AutoResetEvent SererInitialized;
        private bool isMyBackgroundTaskRunning = false;

        public static Player Instance = new Player();

        public event EventHandler<MediaPlayerState> StateChanged;

        public MediaPlayerState State {
            get { return BackgroundMediaPlayer.Current.CurrentState; }
        }

        public Player()
        {
            SererInitialized = new AutoResetEvent(false);

            App.Current.Suspending += ForegroundApp_Suspending;
            App.Current.Resuming += ForegroundApp_Resuming;

        }

        #region Foreground App Lifecycle Handlers
        /// <summary>
        /// Sends message to background informing app has resumed
        /// Subscribe to MediaPlayer events
        /// </summary>
        void ForegroundApp_Resuming(object sender, object e)
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
        private bool IsMyBackgroundTaskRunning
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

        /// <summary>
        /// Initialize Background Media Player Handlers and starts playback
        /// </summary>
        private void StartBackgroundAudioTask(string filePath)
        {
            if (isMyBackgroundTaskRunning)
            {
                var message = new ValueSet();
                message.Add(Constants.AddTrack, filePath);
                BackgroundMediaPlayer.SendMessageToBackground(message);

                message = new ValueSet();
                message.Add(Constants.StartPlayback, "0");
                BackgroundMediaPlayer.SendMessageToBackground(message);
            }
            else
            {
                AddMediaPlayerEventHandlers();
                var backgroundtaskinitializationresult = Window.Current.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                {
                    bool result = SererInitialized.WaitOne(2000);
                    //Send message to initiate playback
                    if (result == true)
                    {
                        var message = new ValueSet();
                        message.Add(Constants.AddTrack, filePath);
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
                backgroundtaskinitializationresult.Completed = new AsyncActionCompletedHandler(BackgroundTaskInitializationCompleted);
            }
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
        #endregion

        #region Background MediaPlayer Event handlers
        /// <summary>
        /// MediaPlayer state changed event handlers. 
        /// Note that we can subscribe to events even if Media Player is playing media in background
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        /*async*/ void MediaPlayer_CurrentStateChanged(MediaPlayer sender, object args)
        {           
            if (StateChanged != null)
            {
                StateChanged(this, sender.CurrentState);
            }
        }

        /// <summary>
        /// This event fired when a message is recieved from Background Process
        /// </summary>
        /*async*/ void BackgroundMediaPlayer_MessageReceivedFromBackground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            foreach (string key in e.Data.Keys)
            {
                switch (key)
                {
                    case Constants.Trackchanged:
                        //When foreground app is active change track based on background message
                        //await this.Dispatcher.RunAsync(CoreDispatcherPriority.Normal, () =>
                        //{
                        //    txtCurrentTrack.Text = (string)e.Data[key];
                        //}
                        //);
                        break;
                    case Constants.BackgroundTaskStarted:
                        //Wait for Background Task to be initialized before starting playback
                        Debug.WriteLine("Background Task started");
                        SererInitialized.Set();
                        break;
                }
            }
        }

        #endregion
    }
}
