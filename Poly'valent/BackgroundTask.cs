using Discord;
using Discord.WebSocket;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Mysqlx.Crud;
using Poly_valent.Utils;
using PolyDatabase;
using static Google.Protobuf.Reflection.SourceCodeInfo.Types;

namespace Poly_valent
{
    public class BackgroundTask : IHostedService, IDisposable
    {
        private Timer? _timer = null;
        private readonly TimeSpan _interval = TimeSpan.FromMinutes(5);
        private readonly Servers _servers;
        private readonly PolyDatabase.Grades _grades;
        private readonly Newsletters _newsletters;
        private readonly IConfiguration _config;
        private readonly DiscordSocketClient _client;

        public BackgroundTask(Servers servers, PolyDatabase.Grades grades, Newsletters newsletters, IConfiguration config, DiscordSocketClient client)
        {
            _servers = servers;
            _grades = grades;
            _newsletters = newsletters;
            _config = config;
            _client = client;
        }
        
        public Task StartAsync(CancellationToken c)
        {
            _timer = new Timer(Update, null, TimeSpan.Zero, _interval);
            return Task.CompletedTask;
        }

        private async void Update(object? state)
        {
            Console.WriteLine("Update");
            await UpdateGrades();
            await UpdateEDTs();
        }
        
        public Task StopAsync(CancellationToken c)
        {
            _timer?.Change(Timeout.Infinite, 0);
            Console.WriteLine("Background tasks were cancelled");
            return Task.CompletedTask;
        }
        public void Dispose() => _timer?.Dispose();

        private async Task UpdateGrades()
        {
            List<Server> s = await _servers.GetServersAsync();
            List<Grade> g = await _grades.GetAllGrades();
            if (g.Count > 0)
            {
                List<Test> old_g = g.ConvertAll(ToTestAsync);
#pragma warning disable CS8604 // Possible null reference argument.
                List<Test> new_g = await Utils.Grades.GetGrades(_config.GetValue<string>("studentId"), _config.GetValue<string>("password"), 2);
#pragma warning restore CS8604 // Possible null reference argument.
                List<Test> diff = new_g.Except(old_g, Test.Comparer).ToList();
                if (diff.Count > 0)
                {
                    foreach (Test t in diff)
                    {
                        EmbedBuilder b = new EmbedBuilder()
                            .WithTitle("Update")
                            .AddField("Actuellement disponible sur OASIS:", $"{t._name} - {t._subject} - {t._subject_id}\n{t._date} - {(t._class_avg < 0 ? "-" : t._class_avg)}")
                            .WithFooter($"{DateTime.Now}");
                        Embed e = b.Build();

                        foreach (Server serv in s)
                        {
                            await UpdateGradesMessage(e, serv.Id, serv.NewsChannel);
                        }
                    }
                }
                else
                {
                    Console.WriteLine("No grades to report");
                }
                foreach (Test x in diff)
                {
#pragma warning disable CS8604 // Possible null reference argument.
                    await _grades.AddGrade(x._name, x._subject, x._subject_id, x._grade == null ? -1.0f : (float)x._grade, x._class_avg == null ? -1.0f : (float)x._class_avg, x._rank, x._date_str, x._appr);
#pragma warning restore CS8604 // Possible null reference argument.
                }
            }
            else
            {
#pragma warning disable CS8604 // Possible null reference argument.
                List<Test> new_g = await Utils.Grades.GetGrades(_config.GetValue<string>("studentId"), _config.GetValue<string>("password"), 3);
                foreach (var x in new_g)
                {
                    await _grades.AddGrade(x._name, x._subject, x._subject_id, x._grade == null ? -1.0f : (float)x._grade, x._class_avg == null ? -1.0f : (float)x._class_avg, x._rank, x._date_str, x._appr);
                }
                Console.WriteLine("Initial start");
#pragma warning restore CS8604 // Possible null reference argument.
            }

        }

        private async Task UpdateEDTs()
        {
            List<Newsletter> l = await _newsletters.GetAllNewslettersAsync();
#pragma warning disable CS8604 // Possible null reference argument.
            string y = await Utils.Grades.GetCurrentSchoolYear(_config.GetValue<string>("studentId"), _config.GetValue<string>("password"));
#pragma warning restore CS8604 // Possible null reference argument.
            foreach (Newsletter n in l)
            {
                Calendar old_c = cal.ToCalendar(n.Calendar ?? "");
                Calendar new_c = cal.GetEDT(n.Id, DateTime.UtcNow, new DateTime(int.Parse(y)+1, 7, 28));
                List<CalendarEvent> l1 = old_c.Events.ToList();
                List<CalendarEvent> l2 = new_c.Events.ToList();
                l1.ForEach(x => x.Description = string.Join("\n",x.Description.Remove(0, 2).Split().SkipLast(2)));
                l2.ForEach(x => x.Description = string.Join("\n", x.Description.Remove(0, 2).Split().SkipLast(2)));
                List<CalendarEvent> l_diff1 = l2.Except(l1).ToList();
                if (l_diff1.Count > 0)
                {
                    foreach (CalendarEvent ev in l_diff1)
                    {

                        string[] desc = ev.Description.Split('\n');
                        EmbedBuilder b = new EmbedBuilder()
                            .WithTitle("Nouveau cours:")
                            .AddField("Cours", ev.Summary ?? " ", false)
                            .AddField("Professeur(s)", desc[0] ?? " ", false)
                            .AddField("Participant(s)", string.Join("\n", desc.Skip(1)) ?? " ")
                            .AddField("Date", ev.Start.ToTimeZone("France/Paris").ToString() ?? " ")
                            .AddField("Lieu", ev.Location ?? " ")
                            .WithFooter($"{DateTime.Now}");
                        await UpdateEDTMessage(b.Build(), n.UserId);
                    }
                }
                List<CalendarEvent> l_diff2 = l1.Except(l2).ToList();
                if (l_diff2.Count > 0) 
                {
                    foreach (CalendarEvent ev in l_diff2)
                    {

                        string[] desc = ev.Description.Split('\n');
                        EmbedBuilder b = new EmbedBuilder()
                            .WithTitle("Cours supprimé:")
                            .AddField("Cours", ev.Summary ?? " ", false)
                            .AddField("Professeur(s)", desc[0] ?? " ", false)
                            .AddField("Participant(s)", string.Join("\n", desc.Skip(1)) ?? " ")
                            .AddField("Date", ev.Start.ToTimeZone("France/Paris").ToString() ?? " ")
                            .AddField("Lieu", ev.Location ?? " ")
                            .WithFooter($"{DateTime.Now}");
                        await UpdateEDTMessage(b.Build(), n.UserId);
                    }
                }
                if (l_diff1.Count > 0 || l_diff2.Count > 0)
                {
                    await _newsletters.ChangeCalendar(n.UserId, cal.GetEDTString(n.Id, DateTime.UtcNow, new DateTime(int.Parse(y) + 1, 7, 28)));

                    Console.WriteLine("New EDT");
                }
                else
                {
                    Console.WriteLine("No new EDT");
                }
            }
        }

        private Test ToTestAsync(Grade g)
        {
#pragma warning disable CS8604 // Possible null reference argument.
            return  new Test(g.Name, g.Subject, g.Subject_id, g.grade, g.Class_avg, g.Rank, g.Date_str, g.Appr);
#pragma warning restore CS8604 // Possible null reference argument.
        }
        public async Task UpdateGradesMessage(Embed e, ulong id, ulong channel)
        {
            SocketGuild g = _client.GetGuild(id);
            SocketTextChannel c = g.GetTextChannel(channel);
            await c.SendMessageAsync(embed: e);
        }
        public async Task UpdateEDTMessage(Embed e, ulong id)
        {
            IUser u = await _client.GetUserAsync(id);
            IDMChannel c = u.CreateDMChannelAsync().Result;
            await c.SendMessageAsync(embed: e);
            await Task.Delay(1000);
        }

    }
}
