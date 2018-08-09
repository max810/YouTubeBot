using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using YouTubeBot.ConfigurationProviders;
using YouTubeBot.Models;

namespace YouTubeBot
{
    // choose between static methods with parameters or pass everything in constructor and save
    public class YoutubeVideoDownloader
    {
        private string youtubeUrl;
        private VideoDownloadConfig config;
        public YoutubeVideoDownloader(VideoDownloadConfig queryConfig, string _youtubeUrl)
        {
            // var info = GetVideoInfo();

            // do (try get links) while (not successfull && not all services tried)
            // -> get page
            // -> parse with AgilityPack

            

            // later in formatter try use Inline keyboard with just (download url buttons), maybe will work out
            // else = callbackButtons with (sendFile or sendVideo)
        }

        // maybe private, will be used in
        public VideoInfo GetVideoInfo()
        {
            // use youtubeUrl here
            throw new NotImplementedException();
        }

        // <mp4 40.5 mb, http://...downloadlink>
        // maybe use struct (or class) like { FileExtension, EstimatedSize, Quality(720p) }
        public IDictionary<string, string> GetDownloadLinks()
        {
            throw new NotImplementedException();
        }
    }
}
