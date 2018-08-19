using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using YouTubeBot.Models;

namespace YouTubeBot
{
    public static class ResponseFormatter
    {
        public static IEnumerable<KeyValuePair<string, IReplyMarkup>> GetFormattedResponse(IEnumerable<DownloadLink> downloadLinks)
        {
            if (!downloadLinks.Any())
            {
                return new[] 
                {
                    // 'try again (repeat)' button
                    new KeyValuePair<string, IReplyMarkup>(
                        "_Sorry, it looks like there are no download links available for this video =(_", 
                        InlineKeyboardMarkup.Empty())
                };
            }

            var response = new List<KeyValuePair<string, IReplyMarkup>>();

            var fileTypes = downloadLinks.GroupBy(x => x.FileFormat);

            foreach(var fileType in fileTypes)
            {
                var markup = GetKeyboard(fileType);
                string messageText = $"Download **{fileType.Key}**";

                response.Add(new KeyValuePair<string, IReplyMarkup>(messageText, markup));
            }

            return response;
        }

        public static IReplyMarkup GetKeyboard(IEnumerable<DownloadLink> downloadLinks)
        {
            List<List<InlineKeyboardButton>> keyboard = new List<List<InlineKeyboardButton>>();
            foreach(var downloadLink in downloadLinks)
            {
                List<InlineKeyboardButton> keyboardRow = new List<InlineKeyboardButton>(1);
                InlineKeyboardButton button = InlineKeyboardButton
                    .WithUrl(downloadLink.Quality + " " + downloadLink.FileSize, downloadLink.Link);

                keyboardRow.Add(button);
                keyboard.Add(keyboardRow);
            }

            return new InlineKeyboardMarkup(keyboard);
        }
    }

}
