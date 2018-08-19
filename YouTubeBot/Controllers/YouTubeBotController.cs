using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text.RegularExpressions;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;
using YouTubeBot.ConfigurationProviders;
using YouTubeBot.Models;

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
            logger.LogCritical(CurrentHostUri);
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
            }
            catch (ArgumentException)
            {
                await bot.SendTextMessageAsync(chatId,
                    "_Sorry, I don't understand you.\n I **only** understand links from **YouTube**._",
                    ParseMode.Markdown);
                return;
            }

            string fullVideoUrl = update.Message.Text;
            IList<DownloadLink> links;
            try
            {
                links = await YoutubeVideoDownloader.GetDownloadLinksAsync(
                    videoDownloadSettings,
                    fullVideoUrl,
                    () => OnNotLoading(chatId),
                    shortenLinks: true,
                    bitLySettings: bitLySettings
            );
            }
            catch (HttpRequestException)
            {
                await bot.SendTextMessageAsync(chatId, 
                    "_Sorry, looks like the link you provided is **invalid**.\nOr the service is down =( Maybe try later._", 
                    ParseMode.Markdown);
                return;
            }

            var response = ResponseFormatter.GetFormattedResponse(links);

            foreach (var kv in response)
            {
                await bot.SendTextMessageAsync(chatId, kv.Key, parseMode: ParseMode.Markdown, replyMarkup: kv.Value);
            }
        }

        private async void OnNotLoading(long chatId)
        {
            await bot.SendTextMessageAsync(chatId, "_Loading, please wait..._", ParseMode.Markdown);
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
    }
}
