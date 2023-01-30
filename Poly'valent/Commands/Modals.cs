using Discord.Interactions;
using Microsoft.Extensions.Configuration;
using PolyDatabase;

namespace Poly_valent.Commands
{
    public class Modals : InteractionModuleBase<SocketInteractionContext>
    {
        private readonly Newsletters _newsletters;
        private readonly IConfiguration _configuration;
        public Modals(Newsletters newsletters, IConfiguration configuration)
        {
            _newsletters = newsletters;
            _configuration = configuration;
        }
        
        [SlashCommand("edtupdates", "The bot will warn you about edt changes")]
        public async Task AddEDTPerson() => await Context.Interaction.RespondWithModalAsync<AddEDT>("edt");

        [ModalInteraction("edt")]
        public async Task ModalResponse(AddEDT modal)
        {
            bool isValid = ulong.TryParse(modal.Id, out ulong id);
            if (!isValid)
            {
                await RespondAsync("Invalid id");
                return;
            }
#pragma warning disable CS8604 // Possible null reference argument.
            string y = await Utils.Grades.GetCurrentSchoolYear(_configuration.GetValue<string>("studentId"), _configuration.GetValue<string>("password"));
#pragma warning restore CS8604 // Possible null reference argument.
            if (await _newsletters.ExistsInDB(Context.User.Id))
                await _newsletters.ChangeIdAsync(Context.User.Id, id);
            else
                await _newsletters.AddNewsletterAsync(Context.User.Id, id, Utils.cal.GetEDTString(id, DateTime.UtcNow, new DateTime(int.Parse(y), 7, 28)));
            await RespondAsync($"you'll receive all news of the EDT with the following id: {modal.Id}");
        }

        public class AddEDT : IModal
        {
            public string Title => "Ajout/changement newsletter edt";

            [InputLabel("Votre id? /help_edt for help")]
            [ModalTextInput("Id", placeholder: "AAAA", minLength:1, maxLength:7)]
            public string? Id { get; set; }
        }
    }

}
