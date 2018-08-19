using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YouTubeBot.Models
{
    public class VideoProvider
    {
        public string UrlFormat { get; set; }
        public DownloadLinkInfo[] DownloadLinksInfo { get; set; }
    }
}
