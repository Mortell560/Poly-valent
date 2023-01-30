using Discord;
using Discord.Interactions;
using PolyDatabase;

namespace Poly_valent.Commands
{
    public class PolyCommands : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly Servers _servers;
        private readonly Newsletters _newsletters;
        private readonly Grades _grades;

        public PolyCommands(Servers servers, Newsletters newsletters, Grades grades)
        {
            _servers = servers;
            _newsletters = newsletters;
            _grades = grades;
        }

        [SlashCommand("set_news_channel", "change the news channel for this server to the db (adds it if it doesn't exist)")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task ChangeChannelToDB(IMessageChannel c)
        {
            if (!await _servers.ExistsInDB(Context.Guild.Id))
            {
                await _servers.AddServerAsync(Context.Guild.Id, c.Id);
            }
            else
            {
                await _servers.ChangeNewsChannel(Context.Guild.Id, c.Id);
            }
            await RespondAsync("done");
        }

        [SlashCommand("remove_from_db", "quite explicit")]
        [RequireUserPermission(GuildPermission.Administrator)]
        public async Task RemoveFromDB()
        {
            if (await _servers.ExistsInDB(Context.Guild.Id))
            {
                await _servers.RemoveServerAsync(Context.Guild.Id);
            }
            await RespondAsync("done");
        }

        [SlashCommand("remove_from_edt", "removes you from the newsletter for the edt")]
        public async Task RemoveFromNewsletterDB()
        {
            if (await _newsletters.ExistsInDB(Context.User.Id))
            {
                await _newsletters.RemoveNewsletterAsync(Context.User.Id);
            }
            await RespondAsync("done");
        }

    }
}
