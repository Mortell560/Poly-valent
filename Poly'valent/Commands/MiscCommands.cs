using Discord;
using Discord.Interactions;
using Ical.Net;
using Ical.Net.CalendarComponents;

namespace Poly_valent.Commands
{
    public class MiscCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("nextclass", "tells you your next class", false, RunMode.Async)]
        public async Task NextClass(int id)
        {
            Calendar c = Utils.cal.GetEDT(id, DateTime.Now, DateTime.Now.AddDays(2));
            if (c == null)
            {
                await RespondAsync("Tu n'as pas de cours pour 2j au moins");
                return;
            }
            CalendarEvent? e = Utils.cal.nextClass(c, DateTime.Now);
#pragma warning disable CS8602 // Dereference of a possibly null reference.
            string[] desc = e.Description.Skip(4).SkipLast(32).ToString().Split("\n");
#pragma warning restore CS8602 // Dereference of a possibly null reference.
            EmbedBuilder b = new EmbedBuilder()
                .WithTitle("Prochain cours:")
                .AddField("Cours", e.Summary, false)
                .AddField("Professeur(s)", desc[0], false)
                .AddField("Participant(s)", string.Join("\n",desc.Skip(0)))
                .AddField("Date", e.Start.ToTimeZone("France/Paris").ToString())
                .WithFooter($"Command executed by {Context.User.Username} at {DateTime.Now}", Context.User.GetAvatarUrl() ?? Context.User.GetDefaultAvatarUrl());
            await RespondAsync(null, embed: b.Build());
            
        }
    }
}
