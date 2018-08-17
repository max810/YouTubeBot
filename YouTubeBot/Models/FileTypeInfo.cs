using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YouTubeBot.Models
{
    public class FileTypeInfo
    {
        public string FileType { get; set; }
        public DownloadLinkInfo[] DownloadLinksInfo { get; set; }
    }
}
