using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.Text;
using System.Threading.Tasks;

namespace ACastShared.Messages
{
    [DataContract]
    public class SetSleepTimerMessage
    {
        [DataMember]
        public int DurationMs;

        public SetSleepTimerMessage(int durationMs) {
            this.DurationMs = durationMs;
        }
    }
}
