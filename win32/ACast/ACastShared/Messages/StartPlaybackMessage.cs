﻿//*********************************************************
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
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ACastShared.Messages
{
    [DataContract]
    public class StartPlaybackMessage
    {
    }

    public class ResumePlaybackMessage
    {
    }


    [DataContract]
    public class StartTrackMessage
    {
        public StartTrackMessage()
        {
        }

        public StartTrackMessage(Uri trackId)
        {
            this.TrackId = trackId;
        }

        [DataMember]
        public Uri TrackId;
    }
}
