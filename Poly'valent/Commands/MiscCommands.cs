using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Poly_valent.Utils;
using PolyDatabase;

namespace Poly_valent.Commands
{
    public class MiscCommands : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly DiscordSocketClient _client;
        private readonly IHost _host;
        private readonly Newsletters _newsletters;
        private readonly IConfiguration _configuration;

        public MiscCommands(DiscordSocketClient client, IHost host, IConfiguration configuration, Newsletters newsletters)
        {
            _client = client;
            _host = host;
            _configuration = configuration;
            _newsletters = newsletters;
        }


        [SlashCommand("newsletter_edt", "will warn you about edt changes")]
        public async Task SetNewsletter_edt(ulong id)
        {
            await DeferAsync();
#pragma warning disable CS8604 // Possible null reference argument.
            string y = await Utils.Grades.GetCurrentSchoolYear(_configuration.GetValue<string>("studentId"), _configuration.GetValue<string>("password"));
#pragma warning restore CS8604 // Possible null reference argument.
            if (await _newsletters.ExistsInDB(Context.User.Id))
            {
                await _newsletters.RemoveNewsletterAsync(Context.User.Id); // If we don't put this the user will get spammed once they change
                await _newsletters.AddNewsletterAsync(Context.User.Id, id, cal.GetEDTString(id, DateTime.UtcNow, new DateTime(int.Parse(y) + 1, 7, 28)));
            }
            else
                await _newsletters.AddNewsletterAsync(Context.User.Id, id, cal.GetEDTString(id, DateTime.UtcNow, new DateTime(int.Parse(y)+1, 7, 28)));
            await FollowupAsync($"you'll receive all news of the EDT with the following id: {id}");
        }

        [SlashCommand("nextclass", "tells you your next class", false, RunMode.Async)]
        public async Task NextClass(ulong id)
        {
            await DeferAsync();
            Calendar c = cal.GetEDT(id, DateTime.Now, DateTime.Now.AddDays(3));
            if (c == null)
            {
                await FollowupAsync("Tu n'as pas de cours pour 3j au moins");
                return;
            }
            CalendarEvent? e = cal.nextClass(c, DateTime.Now);

            if(e == null)
            {
                await FollowupAsync("Tu n'as pas de prochains cours");
                return;
            }

            string descTemp = e.Description.Remove(0,2);
            string[] desc = descTemp.Split('\n');
            EmbedBuilder b = new EmbedBuilder()
                .WithTitle("Prochain cours:")
                .AddField("Cours", e.Summary, false)
                .AddField("Professeur(s)", desc[0], false)
                .AddField("Participant(s)", string.Join("\n",desc.Skip(1).SkipLast(2)))
                .AddField("Date", e.Start.ToTimeZone("France/Paris").ToString())
                .AddField("Lieu", e.Location)
                .WithFooter($"Command executed by {Context.User.Username} at {DateTime.Now}", Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl());
            await FollowupAsync(null, embed: b.Build());
            
        }

        [SlashCommand("findoccupiedroom", "finds all occupied rooms", false, RunMode.Async)]
        public async Task FindOccupiedAsync(string bats)
        {
            List<string> list = new();
            list.AddRange(bats.Split(" ").Where(x => cal.Ids_bat.ContainsKey(x)));
            await DeferAsync(); // The command needs about 3-5 seconds to answer and 3s is the max delay to acknowledge an interaction hence why we're using this
            DateTime d = DateTime.Now;

            List<string> rooms = cal.FindCurrentlyOccupiedRoom(list, d);

            EmbedBuilder builder = new EmbedBuilder()
                .WithTitle("Salles occupées dans les bâtiments suivants:")
                .WithFooter($"Command executed by {Context.User.Username} at {DateTime.Now}", Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl());
            foreach(var room in rooms)
            {
                builder.AddField(room, "");
            }
            await FollowupAsync(null, embed: builder.Build());
        }

        [SlashCommand("gettests", "Owner only")]
        [RequireOwner]
        public async Task GetTestsAsync(int s, string? sortName = null)
        {
            await DeferAsync();
#pragma warning disable CS8604 // Possible null reference argument.
            List<Test> c = await Utils.Grades.GetGrades(_configuration.GetValue<string>("studentId"), _configuration.GetValue<string>("password"), s);
#pragma warning restore CS8604 // Possible null reference argument.
            string desc = "";
            c = c.OrderBy(x => x._date).ToList();
            if (sortName != null)
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                c = c.FindAll(x => x._subject.Contains(sortName));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }
            foreach (var course in c)
            {
                desc += course.ToString() + "\n";
            }
            EmbedBuilder b = new EmbedBuilder()
                .WithTitle("Tes notes:")
                .WithDescription(desc)
                .WithFooter($"Command executed by {Context.User.Username} at {DateTime.Now}", Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl());
            await FollowupAsync(null, embed: b.Build(), ephemeral: true);
        }

        [SlashCommand("getmodules", "Owner only")]
        [RequireOwner]
        public async Task GetModulesAsync(int s, string? sortName = null)
        {
            await DeferAsync();
#pragma warning disable CS8604 // Possible null reference argument.
            Dictionary<string, List<Module>> d = await Utils.Grades.GetModules(_configuration.GetValue<string>("studentId"), _configuration.GetValue<string>("password"), s);
#pragma warning restore CS8604 // Possible null reference argument.
            string desc = "";
            if (sortName != null)
            {
                d = d.Where(x => x.Key.Contains(sortName)).ToDictionary(x => x.Key, x => x.Value);
            }
            foreach (string key in d.Keys){
                desc += $"**{key}**\n";
                foreach (Module m in d[key])
                {
                    desc += m.ToString() + "\n";
                }
            }
            EmbedBuilder b = new EmbedBuilder()
                .WithTitle("Tes modules:")
                .WithDescription(desc)
                .WithFooter($"Command executed by {Context.User.Username} at {DateTime.Now}", Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl());
            await FollowupAsync(null, embed: b.Build(), ephemeral: true);
        }

        [SlashCommand("getues", "Owner only")]
        [RequireOwner]
        public async Task GetUesAsync(int s, string? sortName = null)
        {
            await DeferAsync();
#pragma warning disable CS8604 // Possible null reference argument.
            List<UE> c = await Utils.Grades.GetUE(_configuration.GetValue<string>("studentId"), _configuration.GetValue<string>("password"), s);
#pragma warning restore CS8604 // Possible null reference argument.
            string desc = "";
            c = c.OrderBy(x => x._code).ToList();
            if (sortName != null)
            {
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                c = c.FindAll(x => x._name.Contains(sortName));
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            }
            foreach (UE ue in c)
            {
                desc += ue.ToString() + "\n";
            }
            EmbedBuilder b = new EmbedBuilder()
                .WithTitle("Tes UEs:")
                .WithDescription(desc)
                .WithFooter($"Command executed by {Context.User.Username} at {DateTime.Now}", Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl());
            await FollowupAsync(null, embed: b.Build(), ephemeral: true);
        }

        [SlashCommand("help_edt", "Helps you ?", false, RunMode.Async)]
        public async Task HelpEdtAsync()
        {
            await RespondAsync("https://mortell560.notion.site/Aide-pour-trouver-id-7515ae00658b4b2e917228c8e5b2407f");
        }


        [SlashCommand("shut", "shuts the bot", false, RunMode.Async)]
        [RequireOwner]
        public async Task Exit()
        {
            await DeferAsync();
            await FollowupAsync("im ded");
            await _client.LogoutAsync();
            await _host.StopAsync();
        }
    }
}
