/*
 * (c) Copyright Microsoft Corporation.
This source is subject to the Microsoft Public License (Ms-PL).
All other rights reserved.
 */
using System;
using System.Diagnostics;
using System.Threading;
using Windows.ApplicationModel.Background;
using Windows.Media;
using Windows.Media.Playback;
using Windows.Foundation.Collections;
using Windows.Storage;
using ACast;

/* This is the Sample background task that will start running the first time 
 * MediaPlayer singleton instance is accessed from foreground. When a new audio 
 * or video app comes into picture the task is expected to recieve the cancelled 
 * event. User can save state and shutdown MediaPlayer at that time. When foreground 
 * app is resumed or restarted check if your music is still playing or continue from
 * previous state.
 * 
 * This task also implements SystemMediaTransportControl apis for windows phone universal 
 * volume control. Unlike Windows 8.1 where there are different views in phone context, 
 * SystemMediaTransportControl is singleton in nature bound to the process in which it is 
 * initialized. If you want to hook up volume controls for the background task, do not 
 * implement SystemMediaTransportControls in foreground app process.
 */

namespace ACastBackgroundAudioTask
{
    /// <summary>
    /// Enum to identify foreground app state
    /// </summary>
    enum ForegroundAppStatus
    {
        Active,
        Suspended,
        Unknown
    }

    /// <summary>
    /// Impletements IBackgroundTask to provide an entry point for app code to be run in background. 
    /// Also takes care of handling UVC and communication channel with foreground
    /// </summary>
    public sealed class BackgroundAudioTask : IBackgroundTask
    {

        private SystemMediaTransportControls systemmediatransportcontrol;
        private BackgroundTaskDeferral deferral; // Used to keep task alive
        private ForegroundAppStatus foregroundAppState = ForegroundAppStatus.Unknown; 
        private AutoResetEvent BackgroundTaskStarted = new AutoResetEvent(false);
        private bool backgroundtaskrunning = false;

        private string currentMediaPath;

        public BackgroundAudioTask()
        {

        }

        /// <summary>
        /// The Run method is the entry point of a background task. 
        /// </summary>
        /// <param name="taskInstance"></param>
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            Debug.WriteLine("Background Audio Task " + taskInstance.Task.Name + " starting...");
            // Initialize SMTC object to talk with UVC. 
            //Note that, this is intended to run after app is paused and 
            //hence all the logic must be written to run in background process
            systemmediatransportcontrol = SystemMediaTransportControls.GetForCurrentView();
            systemmediatransportcontrol.ButtonPressed += systemmediatransportcontrol_ButtonPressed;
            systemmediatransportcontrol.PropertyChanged += systemmediatransportcontrol_PropertyChanged;
            systemmediatransportcontrol.IsEnabled = true;
            systemmediatransportcontrol.IsPauseEnabled = true;
            systemmediatransportcontrol.IsPlayEnabled = true;
            systemmediatransportcontrol.IsNextEnabled = false;
            systemmediatransportcontrol.IsPreviousEnabled = false;
            //systemmediatransportcontrol.IsFastForwardEnabled = true;
            

            // Associate a cancellation and completed handlers with the background task.
            taskInstance.Canceled += new BackgroundTaskCanceledEventHandler(OnCanceled);
            taskInstance.Task.Completed += Taskcompleted;

            var value = ApplicationSettingsHelper.ReadResetSettingsValue(Constants.AppState);
            if (value == null)
                foregroundAppState = ForegroundAppStatus.Unknown;
            else
                foregroundAppState = (ForegroundAppStatus)Enum.Parse(typeof(ForegroundAppStatus), value.ToString());

            //Add handlers for MediaPlayer
            BackgroundMediaPlayer.Current.CurrentStateChanged += Current_CurrentStateChanged;

            //Initialize message channel 
            BackgroundMediaPlayer.MessageReceivedFromForeground += BackgroundMediaPlayer_MessageReceivedFromForeground;
            
            //Send information to foreground that background task has been started if app is active
            if (foregroundAppState != ForegroundAppStatus.Suspended)
            {
                ValueSet message = new ValueSet();
                message.Add(Constants.BackgroundTaskStarted, "");
                BackgroundMediaPlayer.SendMessageToForeground(message);
            }
            BackgroundTaskStarted.Set();
            backgroundtaskrunning = true;

            ApplicationSettingsHelper.SaveSettingsValue(Constants.BackgroundTaskState, Constants.BackgroundTaskRunning);
            deferral = taskInstance.GetDeferral();           
        }

        /// <summary>
        /// Indicate that the background task is completed.
        /// </summary>       
        void Taskcompleted(BackgroundTaskRegistration sender, BackgroundTaskCompletedEventArgs args)
        {
            Debug.WriteLine("MyBackgroundAudioTask " + sender.TaskId + " Completed...");
            deferral.Complete();
        }

        /// <summary>
        /// Handles background task cancellation. Task cancellation happens due to :
        /// 1. Another Media app comes into foreground and starts playing music 
        /// 2. Resource pressure. Your task is consuming more CPU and memory than allowed.
        /// In either case, save state so that if foreground app resumes it can know where to start.
        /// </summary>
        private void OnCanceled(IBackgroundTaskInstance sender, BackgroundTaskCancellationReason reason)
        {
            // You get some time here to save your state before process and resources are reclaimed
            Debug.WriteLine("MyBackgroundAudioTask " + sender.Task.TaskId + " Cancel Requested...");
            try
            {
                //save state
                ApplicationSettingsHelper.SaveSettingsValue(Constants.CurrentTrack, "Test" /*Playlist.CurrentTrackName*/);
                ApplicationSettingsHelper.SaveSettingsValue(Constants.Position, BackgroundMediaPlayer.Current.Position.ToString());
                ApplicationSettingsHelper.SaveSettingsValue(Constants.BackgroundTaskState, Constants.BackgroundTaskCancelled);
                ApplicationSettingsHelper.SaveSettingsValue(Constants.AppState, Enum.GetName(typeof(ForegroundAppStatus), foregroundAppState));
                backgroundtaskrunning = false;
                //unsubscribe event handlers
                systemmediatransportcontrol.ButtonPressed -= systemmediatransportcontrol_ButtonPressed;
                systemmediatransportcontrol.PropertyChanged -= systemmediatransportcontrol_PropertyChanged;
                
                //clear objects task cancellation can happen uninterrupted
                BackgroundMediaPlayer.Shutdown(); // shutdown media pipeline
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.ToString());
            }
            deferral.Complete(); // signals task completion. 
            Debug.WriteLine("MyBackgroundAudioTask Cancel complete...");
        }
        
        /// <summary>
        /// Update UVC using SystemMediaTransPortControl apis
        /// </summary>
        private void UpdateUVCOnNewTrack()
        {
            systemmediatransportcontrol.PlaybackStatus = MediaPlaybackStatus.Playing;
            systemmediatransportcontrol.DisplayUpdater.Type = MediaPlaybackType.Music;
            //systemmediatransportcontrol.DisplayUpdater.MusicProperties.Title = Playlist.CurrentTrackName;
            systemmediatransportcontrol.DisplayUpdater.Update();
        }

        /// <summary>
        /// Fires when any SystemMediaTransportControl property is changed by system or user
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        void systemmediatransportcontrol_PropertyChanged(SystemMediaTransportControls sender, SystemMediaTransportControlsPropertyChangedEventArgs args)
        {
            //TODO: If soundlevel turns to muted, app can choose to pause the music
        }

        /// <summary>
        /// This function controls the button events from UVC.
        /// This code if not run in background process, will not be able to handle button pressed events when app is suspended.
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="args"></param>
        private void systemmediatransportcontrol_ButtonPressed(SystemMediaTransportControls sender, SystemMediaTransportControlsButtonPressedEventArgs args)
        {
            switch (args.Button)
            {
                case SystemMediaTransportControlsButton.Play: 
                    Debug.WriteLine("UVC play button pressed");
                    // If music is in paused state, for a period of more than 5 minutes, 
                    //app will get task cancellation and it cannot run code. 
                    //However, user can still play music by pressing play via UVC unless a new app comes in clears UVC.
                    //When this happens, the task gets re-initialized and that is asynchronous and hence the wait
                    if (!backgroundtaskrunning)
                    {
                        bool result = BackgroundTaskStarted.WaitOne(2000);
                        if (!result)
                            throw new Exception("Background Task didnt initialize in time");
                    }
                    //StartPlayback();
                    BackgroundMediaPlayer.Current.Play();
                    break;
                case SystemMediaTransportControlsButton.Pause: 
                    Debug.WriteLine("UVC pause button pressed");
                    try
                    {
                        BackgroundMediaPlayer.Current.Pause();
                    }
                    catch (Exception ex)
                    {
                        Debug.WriteLine(ex.ToString());
                    }
                    break;
                case SystemMediaTransportControlsButton.Next: 
                    Debug.WriteLine("UVC next button pressed");
                    //SkipToNext();
                    break;
                case SystemMediaTransportControlsButton.Previous: 
                    Debug.WriteLine("UVC previous button pressed");
                    //SkipToPrevious();
                    break;
            }
        }

        

        /// <summary>
        /// Start playlist and change UVC state
        /// </summary>

        private void StartPlayback()
        {
            
        }
        
        void Current_CurrentStateChanged(MediaPlayer sender, object args)
        {
            if (sender.CurrentState == MediaPlayerState.Playing)
            {
                systemmediatransportcontrol.PlaybackStatus = MediaPlaybackStatus.Playing;
            }
            else if (sender.CurrentState == MediaPlayerState.Paused)
            {
                systemmediatransportcontrol.PlaybackStatus = MediaPlaybackStatus.Paused;
            }
        }


        /// <summary>
        /// Fires when a message is recieved from the foreground app
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        void BackgroundMediaPlayer_MessageReceivedFromForeground(object sender, MediaPlayerDataReceivedEventArgs e)
        {
            foreach (string key in e.Data.Keys)
            {
                switch (key.ToLower())
                {
                    case Constants.AppSuspended:
                        Debug.WriteLine("App suspending"); // App is suspended, you can save your task state at this point
                        foregroundAppState = ForegroundAppStatus.Suspended;
                        ApplicationSettingsHelper.SaveSettingsValue(Constants.CurrentTrack, "Teset" /*Playlist.CurrentTrackName*/);
                        break;
                    case Constants.AppResumed:
                        Debug.WriteLine("App resuming"); // App is resumed, now subscribe to message channel
                        foregroundAppState = ForegroundAppStatus.Active;
                        break;
                    case Constants.StartPlayback: //Foreground App process has signalled that it is ready for playback
                        Debug.WriteLine("Starting Playback");
                        //StartPlayback();
                        break;
                    case Constants.SkipNext: // User has chosen to skip track from app context.
                        Debug.WriteLine("Skipping to next");
                        //SkipToNext();
                        break;
                    case Constants.SkipPrevious: // User has chosen to skip track from app context.
                        Debug.WriteLine("Skipping to previous");
                        //SkipToPrevious();
                        break;
                    case Constants.AddTrack:                        
                        var en = e.Data.Values.GetEnumerator();
                        en.MoveNext();
                        //Playlist.Tracks.Add(en.Current.ToString());

                        currentMediaPath = en.Current.ToString();
                        BackgroundMediaPlayer.Current.AutoPlay = false;
                        BackgroundMediaPlayer.Current.SetUriSource(new Uri(currentMediaPath));
                        BackgroundMediaPlayer.Current.Play();

                        systemmediatransportcontrol.PlaybackStatus = MediaPlaybackStatus.Playing;
                        systemmediatransportcontrol.DisplayUpdater.Type = MediaPlaybackType.Music;
                        systemmediatransportcontrol.DisplayUpdater.MusicProperties.Title = "Test 1234567890 123456789";
                        systemmediatransportcontrol.DisplayUpdater.Update();
                        break;
                }
            }
        }

    }
}
