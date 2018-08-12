using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YouTubeBot.Models
{
    public class DownloadLink
    {
        public string Description { get; set; }
        public string Link { get; set; }
        public string EstimatedSize { get; set; }

        public DownloadLink()
        {

        }

        public DownloadLink(string description, string link, string size)
        {
            Description = description;
            Link = link;
            EstimatedSize = size;
        }
    }
}
