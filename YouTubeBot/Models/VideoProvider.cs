using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace YouTubeBot.Models
{
    public class VideoProvider
    {
        public string Name { get; set; }
        public FileTypeInfo[] FileTypesInfo { get; set; }
    }
}
