using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YouTubeBot.Models
{
    public class DownloadLinkInfo
    {
        public string DownloadCSSSelector { get; set; }
        public string VideoQualityCSSSelector { get; set; }
        public string FileSizeCSSSelector { get; set; }
    }
}
