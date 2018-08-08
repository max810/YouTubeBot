using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YouTubeBot.ConfigurationProviders
{
    public class LocalDebugConfig
    {
        public bool IsLocalDebug { get; set; }
        public string HttpsUri { get; set; }
    }
}
