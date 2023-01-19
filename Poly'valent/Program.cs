using Discord;
using Discord.Addons.Hosting;
using Discord.Commands;
using Discord.WebSocket;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using System.Text;

namespace Poly_Valent
{
    class Program
    {

        static async Task Main()
        {
            IHostBuilder builder = new HostBuilder()
                .ConfigureAppConfiguration(x =>
                {
                    IConfigurationRoot configuration = new ConfigurationBuilder()
                        .SetBasePath(Directory.GetCurrentDirectory())
                        .AddJsonFile("config.json", false, true)
                        .Build();

                    x.AddConfiguration(configuration);
                })
                .ConfigureLogging(x =>
                {
                    x.AddConsole();
                    x.SetMinimumLevel(LogLevel.Debug);
                })
                .ConfigureDiscordHost((context, config) =>
                {
                    config.SocketConfig = new DiscordSocketConfig
                    {
                        LogLevel = LogSeverity.Verbose,
                        AlwaysDownloadUsers = true,
                        MessageCacheSize = 12,
                    };

                    config.Token = context.Configuration["token"] ?? throw new ArgumentNullException();
                })
                .UseCommandService((context, config) =>
                {
                    config = new CommandServiceConfig()
                    {
                        CaseSensitiveCommands = false,
                        LogLevel = LogSeverity.Verbose,
                        DefaultRunMode = RunMode.Async
                    };
                })
                .UseInteractionService((context, config) =>
                {
                    config.LogLevel = LogSeverity.Info;
                    config.UseCompiledLambda = true;
                })
                .ConfigureServices((context, services) =>
                {
                    services
                    .AddHostedService<CommandHandler>();
                })
                .UseConsoleLifetime();

            IHost host = builder.Build();
            await host.RunAsync();


        }

        /// <summary>
        /// Get bot's configuration (token + prefix) from the config.json file 
        /// </summary>
        /// <returns>ConfigJson</returns>
        public async Task<ConfigJson> GetConfigAsync()
        {
            string json = string.Empty;
            using (FileStream fs = File.OpenRead("config.json"))
            using (StreamReader sr = new StreamReader(fs, new UTF8Encoding(false)))
                json = await sr.ReadToEndAsync();

            return JsonConvert.DeserializeObject<ConfigJson>(json);
        }
    }

}

