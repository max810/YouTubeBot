using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using YouTubeBot.ConfigurationProviders;
using System.Text.RegularExpressions;

namespace YouTubeBot
{
    public static class BitLyApi
    {
        public async static Task<string> ShortenUrlAsync(string longUrl, BitLySettings settings)
        {
            var httpClient = new HttpClient();

            string requestBody = $"{{ \"group_guid\": \"{settings.GroupGuid}\", \"domain\": \"{settings.Domain}\", \"long_url\": \"{longUrl}\" }}";

            var requestContent = new StringContent(requestBody);

            var requestMessage = new HttpRequestMessage(HttpMethod.Post,
                new Uri("https://api-ssl.bitly.com/v4/shorten", UriKind.Absolute));
            requestMessage.Content = requestContent;
            requestMessage.Headers.Add("Authorization", settings.Authorization);
            requestMessage.Headers.Add("origin", "https://bitly.com");
            requestMessage.Headers.Add("referer", "https://bitly.com/");

            requestContent.Headers.Remove("content-type");
            requestContent.Headers.Add("content-type", "application/json");

            string res = await requestMessage.Content.ReadAsStringAsync();

            string shortLink = "error";
            using (var response = await httpClient.SendAsync(requestMessage))
            {
                response.EnsureSuccessStatusCode();
                string responseBody = await response.Content.ReadAsStringAsync();

                Regex linkRegex = new Regex("\"link\":\"(http:\\/\\/bit.ly\\/.{0,8})\"");
                Match linkFound = linkRegex.Match(responseBody);
                string link = linkFound.Groups[1].Value;

                shortLink = linkFound.Value;
            }

            return shortLink;
        }
    }
}
