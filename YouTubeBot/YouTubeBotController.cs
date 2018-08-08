using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using YouTubeBot.ConfigurationProviders;

namespace YouTubeBot.Controllers
{
    [Route("api/[controller]")]
    public class YouTubeBotController : Controller
    {
        private ILogger<YouTubeBotController> logger;
        private NgrokConfig ngrokConfig;

        public YouTubeBotController(ILogger<YouTubeBotController> _logger,
            IOptions<NgrokConfig> options,
            IHostingEnvironment env)
        {
            logger = _logger;
            ngrokConfig = options.Value;
            logger.LogWarning(env.IsDevelopment().ToString());
        }
        // GET api/values
        [HttpGet]
        public IEnumerable<string> Get()
        {
            logger.LogCritical("CRITICAL TEST ONE");
            logger.LogWarning("WARNING test one");
            //logger.LogInformation("\nNgrok config;");
            //logger.LogInformation("{0}", ngrokConfig.Http);
            //logger.LogInformation("{0}", ngrokConfig.Https);
            //logger.LogInformation("\nEnd;");
            return new string[] { "value1", "value2" };
        }

        // GET api/values/5
        [HttpGet("{id}")]
        public string Get(int id)
        {
            return "value";
        }

        // POST api/values
        [HttpPost]
        public void Post([FromBody]string value)
        {
        }
    }
}
