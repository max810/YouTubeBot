using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YouTubeBot.Models
{
    public class FileDownloadLinks
    {
        public string FileType { get; set; }
        public IList<DownloadLink> DownloadLinks { get; set; }
    }
}
