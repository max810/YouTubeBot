using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YouTubeBot.Models
{
    public class DownloadLink
    {
        public string Quality { get; set; }
        public string FileFormat { get; set; }
        public string FileSize { get; set; }
        public string Link { get; set; }

        public DownloadLink()
        {

        }

        public DownloadLink(string quality, string fileFormat, string fileSize, string link)
        {
            Quality = quality;
            FileFormat = fileFormat;
            FileSize = fileSize;
            Link = link;
        }
    }
}
