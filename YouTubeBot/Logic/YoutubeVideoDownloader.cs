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
using System.Net;

namespace YouTubeBot
{
    // choose between static methods with parameters or pass everything in constructor and save
    public static class YoutubeVideoDownloader
    {
        public static async Task<IList<DownloadLink>> GetDownloadLinksAsync(
            VideoDownloadConfig config,
            string videoLink, 
            Action onNotLoading = null,
            bool shortenLinks = false,
            BitLySettings bitLySettings = null)
        {
            List<DownloadLink> result = new List<DownloadLink>();
            var httpClient = new HttpClient();
            var provider = config.VideoProvider;
            // if we didn't get a response in 30 seconds, we throw sorry
            var waitTokenSource = new CancellationTokenSource(TimeSpan.FromSeconds(30));

            string videoLinkEncoded = WebUtility.UrlEncode(videoLink);
            string fullUrl = string.Format(provider.UrlFormat, videoLinkEncoded);

            try
            {
                // will throw if cancelled
                using (var response = await httpClient.GetAsync(fullUrl, waitTokenSource.Token))
                {
                    response.EnsureSuccessStatusCode();

                    var document = new HtmlDocument();
                    document.Load(await response.Content.ReadAsStreamAsync());

                    foreach (var linkInfo in provider.DownloadLinksInfo)
                    {
                        string link = null, quality = null, size = null, format = null;
                        try
                        {
                            quality = document
                                .QuerySelector(linkInfo.VideoQualityCSSSelector)
                                .InnerText;
                            format = document
                                .QuerySelector(linkInfo.FileFormatCSSSelector)
                                .InnerText;
                            size = document
                                .QuerySelector(linkInfo.FileSizeCSSSelector)
                                .InnerText;

                            link = document
                                .QuerySelector(linkInfo.DownloadCSSSelector)
                                .GetAttributeValue("href", def: "error - failed getting href attribute");
                        }
                        catch (NullReferenceException)
                        {
                            continue;
                        }

                        if (shortenLinks)
                        {
                            link = await BitLyApi.ShortenUrlAsync(link, bitLySettings);
                        }

                        result.Add(new DownloadLink(quality, format, size, link));
                    }
                }
            }
            catch (HttpRequestException)
            {
                if (waitTokenSource.Token.IsCancellationRequested)
                {
                    onNotLoading?.Invoke();
                }
                else
                {
                    throw;
                }
            }

            return result;
        }
    }
}
