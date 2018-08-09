using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YouTubeBot.Models;

namespace YouTubeBot.ConfigurationProviders
{
    public class VideoDownloadConfig
    {
        public VideoProvider[] VideoProviders { get; set; }
    }
}
