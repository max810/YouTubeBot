using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
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
        private LocalDebugConfig localDebugConfig;
        private ITelegramBotClient bot;
        private string CurrentHostUri;
        private IHostingEnvironment env;

        public YouTubeBotController(ILogger<YouTubeBotController> _logger,
            IOptions<LocalDebugConfig> options,
            IHostingEnvironment environment,
            ITelegramBotClient botClient)
        {
            logger = _logger;
            localDebugConfig = options.Value;

            bot = botClient;
            env = environment;
            //logger.LogWarning(env.IsDevelopment().ToString());
        }

        [HttpPost]
        [Route("update")]
        public void Update([FromBody] Update update)
        {
            /*
             * try{}
             * catch(){send sorry}
             * send ok
             */

            //bot.SendTextMessageAsync(0, )
            if (update.Type == UpdateType.Message)
            {
                try
                {
                    AssureTextMessage(update.Message);
                    AssureValidYoutubeUrl(update.Message.Text);
                }
                catch (UriFormatException e)
                {
                    // reply "sorry invalid url, e.Message"
                    return;
                }
                catch (ArgumentException e)
                {
                    // reply "don't understand"
                    return;
                }

                string fullUrl = "https://www." + update.Message.Text.StripUrl();

                //bot.SendPhotoAsync()

                // get video info - { title, thumbnail }
                // -> do (try get links) while (not successfull && not all services tried)
                // get download links as IDictionary<key(e.g. "{? 3GP 360p 40.5 Mb }?"), value(url)>
                // get formatted reply (title, thumbnail, links)
                // write message with reply


                // P.S. { ... } - one object


            }
            // main logic here
            // + resolve message type
            // + get url
            // get high quality thumbnail
            // get title
            // get download links
            // format links
            // send thumbnail, title, inline buttons;
        }

        private void AssureValidYoutubeUrl(string link)
        {
            // check for special characters
            AssureValidUri(link);

            string rawAddress = link.StripUrl();
            if (!Regex.IsMatch(rawAddress, @"^youtube\.com\/watch\?v=.{11}&*$"))
            {
                throw new UriFormatException($"This is not a valid youtube video link: {link}");
            }
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

        // GET api/values
        [HttpGet]
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
            if (localDebugConfig.IsLocalDebug)
            {
                // use for debugging with ngrok
                hostUri = localDebugConfig.HttpsUri;
            }

            object currentController = ControllerContext.RouteData.Values["controller"];
            hostUri += $"/{currentController}/update";

            CurrentHostUri = hostUri;

            logger.LogCritical(hostUri);

            return "Bot launched successfully";
        }
    }
}
