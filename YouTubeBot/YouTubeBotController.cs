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
        private static string CurrentHostUri;
        private IHostingEnvironment env;
        private VideoDownloadConfig videoDownloadSettings;
        private BitLySettings bitLySettings;

        public YouTubeBotController(ILogger<YouTubeBotController> _logger,
            IOptions<LocalDebugConfig> localDebugOptions,
            IOptions<VideoDownloadConfig> videoDownloadOptions,
            IOptions<BitLySettings> bitLyOptions,
            IHostingEnvironment environment,
            ITelegramBotClient botClient,
            HttpClient httpClient)
        {
            localDebugSettings = localDebugOptions.Value;
            videoDownloadSettings = videoDownloadOptions.Value;
            bitLySettings = bitLyOptions.Value;

            logger = _logger;
            bot = botClient;
            env = environment;
        }

        [Route("launch")]
        public string Launch()
        {
            logger.LogCritical(CurrentHostUri);
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

            // add <token> to path
            bot.SetWebhookAsync(CurrentHostUri).Wait();

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

            logger.LogCritical("Chat Id = " + chatId);

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
            catch (UriFormatException)
            {
                await bot.SendTextMessageAsync(chatId, "Sorry, the link you sent me was invalid.");
                return;
            }
            catch (ArgumentException)
            {
                await bot.SendTextMessageAsync(chatId,
                    "Sorry, I don't understand you.\n I _only_ understand links from **YouTube**",
                    ParseMode.Markdown);
                return;
            }

            string fullUrl = "https://www." + update.Message.Text.StripUrl();
            string videoID = GetVideoID(fullUrl);

            var links = await YoutubeVideoDownloader.GetDownloadLinksAsync(
                    videoDownloadSettings,
                    videoID,
                    () => OnNotLoading(chatId),
                    shortenLinks: true,
                    bitLySettings: bitLySettings
            );


            var response = ResponseFormatter.GetFormattedResponse(links);

            foreach (var kv in response)
            {
                await bot.SendTextMessageAsync(chatId, kv.Key, parseMode: ParseMode.Markdown, replyMarkup: kv.Value);
            }

        }

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
            bool result = Uri.TryCreate(uriString, UriKind.Absolute, out Uri finalUri)
                && (finalUri.Scheme == Uri.UriSchemeHttp || finalUri.Scheme == Uri.UriSchemeHttps);
            if (!result)
            {
                throw new UriFormatException($"Invalid video link: {uriString}");
            }
        }
    }
}
