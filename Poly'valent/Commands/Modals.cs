using Discord.Interactions;

namespace Poly_valent.Commands
{
    public class Modals : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("AddToEDTUpdates", "The bot will warn you about edt changes")]
        public async Task AddEDTPerson() => await Context.Interaction.RespondWithModalAsync<AddEDT>("addEDT");

        [ModalInteraction("addEDT")]
        public async Task ModalResponse(AddEDT modal)
        {
            await RespondAsync("aaa");
        }

        public class AddEDT : IModal
        {
            public string Title => "Ajout newsletter edt";

            [InputLabel("Votre id? (utilise la commande /help_edt pour savoir comment le trouver si c'est la première fois)")]
            [ModalTextInput("Id", placeholder: "AAAA", minLength:1, maxLength:7)]
            public string? Id { get; set; }
        }
    }

}
