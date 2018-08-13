using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using YouTubeBot.ConfigurationProviders;
using YouTubeBot.Models;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;

namespace YouTubeBot
{
    // choose between static methods with parameters or pass everything in constructor and save
    public static class YoutubeVideoDownloader
    {
        public static event Action NotLoading;

        // maybe private, will be used in
        public static async Task<IEnumerable<FileDownloadLinks>> GetDownloadLinksAsync(VideoDownloadConfig config, string videoID, Action onNotLoading = null)
        {
            var result = new List<FileDownloadLinks>();
            var httpClient = new HttpClient();

            foreach (var provider in config.VideoProviders)
            {
                var waitTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(5));
                string fullUrl = string.Format(provider.UrlFormat, videoID);

                try
                {
                    // will throw if cancelled
                    using (var response = await httpClient.GetAsync(fullUrl, waitTokenSource.Token))
                    {
                        response.EnsureSuccessStatusCode();

                        var document = new HtmlDocument();
                        document.Load(await response.Content.ReadAsStreamAsync());

                        foreach(var fileTypeInfo in provider.FileTypesInfo)
                        {
                            var links = new FileDownloadLinks()
                            {
                                FileType = fileTypeInfo.FileType,
                                DownloadLinks = new List<DownloadLink>()
                            };


                            foreach(var linkInfo in fileTypeInfo.DownloadLinksInfo)
                            {
                                string link = document
                                    .QuerySelector(linkInfo.DownloadCSSSelector)
                                    .GetAttributeValue("href", "error - failed getting href attribute");
                                string description = document
                                    .QuerySelector(linkInfo.DescriptionCSSSelector)
                                    .InnerText;
                                string size = document
                                    .QuerySelector(linkInfo.FileSizeCSSSelector)
                                    .InnerText;

                                links.DownloadLinks.Add(new DownloadLink(description, link, size));
                            }

                            if (links.DownloadLinks.Any())
                            {
                                result.Add(links);
                            }
                        }
                    }
                }
                catch (HttpRequestException)
                {
                    if (waitTokenSource.Token.IsCancellationRequested)
                    {
                        onNotLoading?.Invoke();
                    }
                    continue;
                }
            }

            return result;
        }
    }
}
