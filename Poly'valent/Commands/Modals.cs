using Discord.Interactions;

namespace Poly_valent.Commands
{
    public class Modals : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("addtoedtupdates", "The bot will warn you about edt changes")]
        public async Task AddEDTPerson() => await Context.Interaction.RespondWithModalAsync<AddEDT>("addedt");

        //TODO: bah faire la db et la methode ici
        [ModalInteraction("addedt")]
        public async Task ModalResponse(AddEDT modal)
        {
            await RespondAsync($"aaa{modal.Id}");
        }

        public class AddEDT : IModal
        {
            public string Title => "Ajout newsletter edt";

            [InputLabel("Votre id? /help_edt for help")]
            [ModalTextInput("Id", placeholder: "AAAA", minLength:1, maxLength:7)]
            public string? Id { get; set; }
        }
    }

}
