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
        public static async Task<IEnumerable<FileDownloadLinks>> GetDownloadLinksAsync(VideoDownloadConfig config, string videoID, Action onNotLoading = null, bool shortenLinks = false, BitLySettings bitLySettings = null)
        {
            List<FileDownloadLinks> result = new List<FileDownloadLinks>();
            var httpClient = new HttpClient();

            foreach (var provider in config.VideoProviders)
            {
                result = new List<FileDownloadLinks>();
                var waitTokenSource = new CancellationTokenSource(TimeSpan.FromMinutes(10));
                string fullUrl = string.Format(provider.UrlFormat, videoID);

                try
                {
                    // will throw if cancelled
                    using (var response = await httpClient.GetAsync(fullUrl, waitTokenSource.Token))
                    {
                        response.EnsureSuccessStatusCode();

                        var document = new HtmlDocument();
                        document.Load(await response.Content.ReadAsStreamAsync());

                        foreach (var fileTypeInfo in provider.FileTypesInfo)
                        {
                            var links = new FileDownloadLinks()
                            {
                                FileType = fileTypeInfo.FileType,
                                DownloadLinks = new List<DownloadLink>()
                            };


                            foreach (var linkInfo in fileTypeInfo.DownloadLinksInfo)
                            {
                                string link = document
                                    .QuerySelector(linkInfo.DownloadCSSSelector)
                                    .GetAttributeValue("href", "error - failed getting href attribute");
                                string description = document
                                    .QuerySelector(linkInfo.VideoQualityCSSSelector)
                                    .InnerText;
                                string size = document
                                    .QuerySelector(linkInfo.FileSizeCSSSelector)
                                    .InnerText;

                                if (shortenLinks)
                                {
                                    link = await BitLyApi.ShortenUrlAsync(link, bitLySettings);
                                }

                                links.DownloadLinks.Add(new DownloadLink(description, link, size));
                            }

                            if (links.DownloadLinks.Any())
                            {
                                result.Add(links);
                            }
                        }

                        return result;
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
