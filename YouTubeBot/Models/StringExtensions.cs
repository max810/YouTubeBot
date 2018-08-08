using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace YouTubeBot
{
    public static class StringExtensions
    {
        public static string StripUrl(this string str)
        { 
            // skip protocol 
            string rawAddress = Regex.Split(str, $@"^(https:\/\/|http:\/\/)",
                RegexOptions.Compiled | RegexOptions.IgnoreCase)
                .Last();
            // skip www.
            rawAddress = Regex.Split(str, $@"^www.", RegexOptions.Compiled | RegexOptions.IgnoreCase)
                .Last();

            return rawAddress; 
        }
    }
}
