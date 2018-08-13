using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using YouTubeBot.ConfigurationProviders;

namespace YouTubeBot.Controllers
{
    [Route("[controller]")]
    public class YouTubeBotController : Controller
    {
        private ILogger<YouTubeBotController> logger;
        private LocalDebugConfig localDebugSettings;
        private ITelegramBotClient bot;
        private string CurrentHostUri;
        private IHostingEnvironment env;
        private VideoDownloadConfig videoDownloadSettings;

        public YouTubeBotController(ILogger<YouTubeBotController> _logger,
            IOptions<LocalDebugConfig> localDebugOptions,
            IOptions<VideoDownloadConfig> videoDownloadOptions,
            IHostingEnvironment environment,
            ITelegramBotClient botClient,
            HttpClient httpClient)
        {
            logger = _logger;
            localDebugSettings = localDebugOptions.Value;

            bot = botClient;
            env = environment;

            videoDownloadSettings = videoDownloadOptions.Value;

            // it works; should be '720p'
            //logger.LogCritical((videoDownloadSettings.VideoProviders[0].FileTypesInfo[0].DownloadLinksInfo[0].Description).ToString());
            //logger.LogWarning(env.IsDevelopment().ToString());
        }

        [Route("launch")]
        public string Launch()
        {
            if (!string.IsNullOrWhiteSpace(CurrentHostUri))
            {
                return "Bot is working";
            }

            string hostUri = Request.Host.Host;

            if (!Request.IsHttps)
            {
                hostUri = "https://" + hostUri;
            }
            if (localDebugSettings.IsLocalDebug)
            {
                // use for debugging with ngrok
                hostUri = localDebugSettings.HttpsUri;
            }

            object currentController = ControllerContext.RouteData.Values["controller"];
            hostUri += $"/{currentController}/update";

            CurrentHostUri = hostUri;

            logger.LogCritical(hostUri);

            return "Bot launched successfully";
        }

        [HttpPost]
        [Route("update")]
        public async void Update([FromBody] Update update)
        {
            if (update.Type != UpdateType.Message)
            {
                return;
            }

            long chatId = update.Message.Chat.Id;

            try
            {
                AssureTextMessage(update.Message);
                if (string.Equals(update.Message.Text, "/start", StringComparison.InvariantCultureIgnoreCase))
                {
                    await bot.SendTextMessageAsync(chatId, "Hello! This bot will download videos from YouTube for you.\nJust send me the link.");
                    return;
                }
                AssureValidYoutubeUrl(update.Message.Text);
            }
            catch (UriFormatException e)
            {
                await bot.SendTextMessageAsync(chatId, "Sorry, the link you sent me was invalid.");
                return;
            }
            catch (ArgumentException e)
            {
                await bot.SendTextMessageAsync(chatId, 
                    "Sorry, I don't understand you.\n I _only_ understand links from **YouTube**", 
                    ParseMode.Markdown);
                return;
            }

            string fullUrl = "https://www." + update.Message.Text.StripUrl();
            string videoID = GetVideoID(fullUrl);

            var links = await YoutubeVideoDownloader
                .GetDownloadLinksAsync(videoDownloadSettings, videoID, () => OnNotLoading(chatId));

            var response = ResponseFormatter.GetFormattedResponse(links);

            foreach (var kv in response)
            {
                await bot.SendTextMessageAsync(chatId, kv.Key, parseMode: ParseMode.Markdown, replyMarkup: kv.Value);
            }

            // get video info - { title, thumbnail }
        }
        // main logic here
        // + resolve message type
        // + get url
        // get high quality thumbnail
        // get title
        // get download links
        // format links
        // send thumbnail, title, inline buttons;


        private void AssureValidYoutubeUrl(string link)
        {
            // check for special characters
            AssureValidUri(link);

            string rawAddress = link.StripUrl();
            // can pass any attributes
            if (!Regex.IsMatch(rawAddress, @"^youtube\.com\/watch\?v=.{11}.*$"))
            {
                throw new UriFormatException($"This is not a valid youtube video link: {link}");
            }
        }

        private async void OnNotLoading(long chatId)
        {
            await bot.SendTextMessageAsync(chatId, "_Loading, please wait..._", ParseMode.Markdown);
        }

        private string GetVideoID(string url)
        {
            string query = url.Split('?')[1];

            foreach (var part in query.Split('&'))
            {
                string[] kv = part.Split('=');
                string key = kv[0];
                string value = kv[1];

                if (key == "v")
                {
                    return value;
                }
            }

            throw new ArgumentException("No video id found");
        }

        private void AssureTextMessage(Message message)
        {
            if (message.Type != MessageType.Text)
            {
                throw new ArgumentException($"Wrong message type: {message.Type}"
                    + Environment.NewLine
                    + $"{MessageType.Text} expected.");
            }
        }

        /// <summary>
        /// check for special characters like '+'
        /// </summary>
        /// <param name="uriString"></param>
        private void AssureValidUri(string uriString)
        {
            //Uri finalUri;
            bool result = Uri.TryCreate(uriString, UriKind.Absolute, out Uri finalUri)
                //&& finalUri != null 
                && (finalUri.Scheme == Uri.UriSchemeHttp || finalUri.Scheme == Uri.UriSchemeHttps);
            if (!result)
            {
                throw new UriFormatException($"Invalid video link: {uriString}");
            }
        }
    }
}
