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
        public static IEnumerable<KeyValuePair<string, IReplyMarkup>> GetFormattedResponse(IEnumerable<FileDownloadLinks> downloadLinks)
        {
            if (!downloadLinks.Any())
            {
                return new[] 
                {
                    // 'try again (repeat)' button
                    new KeyValuePair<string, IReplyMarkup>(
                        "Sorry, we couldn't find any video downloads", 
                        InlineKeyboardMarkup.Empty())
                };
            }

            var response = new List<KeyValuePair<string, IReplyMarkup>>();

            foreach(var fileDownload in downloadLinks)
            {
                var markup = FormatLinks(fileDownload);
                string messageText = $"Download **{fileDownload.FileType}**";

                response.Add(new KeyValuePair<string, IReplyMarkup>(messageText, markup));
            }


            // [optional] text: video metadata(title, length, date, channel) 
            // text: Download in *MP4* 
            // inlineButton (link or callback): __720p__
            // inlineButton (link or callback): __480p__
            // repeat

            // if nothing - text: sorry
            // inlineButton: Repeat [basically the same as /start, just ask for the url once again] 

            return response;
        }

        public static IReplyMarkup FormatLinks(FileDownloadLinks downloadLinks)
        {
            List<List<InlineKeyboardButton>> keyboard = new List<List<InlineKeyboardButton>>();
            foreach(var downloadLink in downloadLinks.DownloadLinks)
            {
                List<InlineKeyboardButton> keyboardRow = new List<InlineKeyboardButton>(1);
                InlineKeyboardButton button = InlineKeyboardButton
                    .WithUrl(downloadLink.Description + " " + downloadLink.EstimatedSize, downloadLink.Link);

                keyboardRow.Add(button);
                keyboard.Add(keyboardRow);
            }

            return new InlineKeyboardMarkup(keyboard);
        }
    }

}
