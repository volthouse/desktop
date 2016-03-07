using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ACastShared
{
    public class DebugService
    {
        public List<string> DebugMessages = new List<string>();

        public static DebugService Instance = new DebugService();

        public static void Add(string message)
        {
            Instance.DebugMessages.Add(message);
        }
    }
}
