//*********************************************************
//
// Copyright (c) Microsoft. All rights reserved.
// This code is licensed under the MIT License (MIT).
// THIS CODE IS PROVIDED *AS IS* WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESS OR IMPLIED, INCLUDING ANY
// IMPLIED WARRANTIES OF FITNESS FOR A PARTICULAR
// PURPOSE, MERCHANTABILITY, OR NON-INFRINGEMENT.
//
//*********************************************************

using System;
using System.Diagnostics;
using System.Threading;
using System.Linq;
using Windows.ApplicationModel.Background;
using Windows.Media;
using Windows.Media.Playback;

using System.Collections.Generic;

using ACastShared;
using Windows.Foundation;
using ACastShared.Messages;
using Windows.Storage.Streams;
using Windows.System.Threading;

namespace ACastBackgroundAudioTask
{
    public sealed class MyBackgroundTimerTask : IBackgroundTask
    {
        #region Private fields, properties
     
        private BackgroundTaskDeferral deferral; // Used to keep task alive

        #endregion

        #region IBackgroundTask and IBackgroundTaskInstance Interface Members and handlers
        /// <summary>
        /// The Run method is the entry point of a background task. 
        /// </summary>
        /// <param name="taskInstance"></param>
        public void Run(IBackgroundTaskInstance taskInstance)
        {
            Debug.WriteLine("Background Timer Task " + taskInstance.Task.Name + " starting...");

            deferral = taskInstance.GetDeferral(); // This must be retrieved prior to subscribing to events below which use it

            if (BackgroundMediaPlayer.Current.CurrentState == MediaPlayerState.Playing)
            {
                BackgroundMediaPlayer.Current.Pause();

                ApplicationSettingsHelper.SaveSettingsValue(
                    ApplicationSettingsConstants.Position,
                    BackgroundMediaPlayer.Current.Position.ToString()
                );                
            }           

            deferral.Complete();
        }

        #endregion
    }
}
