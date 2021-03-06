﻿using System.Net.Http;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Telegram.Bot;
using YouTubeBot.ConfigurationProviders;

namespace YouTubeBot
{
    public class Startup
    {
        public Startup(IConfiguration config)
        {
            Configuration = config;
        }

        public IConfiguration Configuration { get; }

        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddMvc();

            string botToken = Configuration.GetSection("YouTubeBot").GetValue<string>("Token");
            services.AddSingleton(typeof(ITelegramBotClient), new TelegramBotClient(botToken));
            services.AddScoped<HttpClient>();
            services.Configure<LocalDebugConfig>(Configuration.GetSection("LocalDebug"));
            services.Configure<VideoDownloadConfig>(Configuration.GetSection("VideoDownloadConfig"));
            services.Configure<BitLySettings>(Configuration.GetSection("BitLyConfig"));

            // not necessary, maybe i'll change Logging to smth else
            services.AddLogging(builder
                => builder
                .AddConfiguration(Configuration.GetSection("Logging"))
                .AddConsole()
            );
        }

        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IHostingEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            app.UseMvc(routes =>
            {
                routes.MapRoute("default", "{controller=YouTubeBot}/{action=Update}");
            });
        }
    }
}
