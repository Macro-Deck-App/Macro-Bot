using Develeon64.MacroBot.Utils;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Develeon64.MacroBot.Commands.Tickets
{
    public class SendTicketMessageCommand : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("sendticketmessage", "Send the base message into the Ticket-Channel")]
        public async Task SendTicketMessage()
        {
            DiscordEmbedBuilder embed = new()
            {
                Title = "__Ticket-Support__",
                Description = "Click on the buttons below to create a ticket.\nPlease select the appropriate topic.",
            };

            SelectMenuBuilder menu = new()
            {
                CustomId = "ticket_create|reason",
                Placeholder = "Select the Reason for your new Ticket",
            };
            menu.AddOption(new SelectMenuOptionBuilder("Can't connect", "no_connect"));
            menu.AddOption(new SelectMenuOptionBuilder("Something is wrong", "wrong_expectation"));
            menu.AddOption(new SelectMenuOptionBuilder("Setup-Assistance", "setup_assistance"));
            menu.AddOption(new SelectMenuOptionBuilder("Problem with a plugin", "plugin_problem"));
            menu.AddOption(new SelectMenuOptionBuilder("Other", "other"));

            ComponentBuilder buttons = new();
            buttons.AddRow(new ActionRowBuilder().WithSelectMenu(menu));

            SocketTextChannel channel = Context.Guild.GetTextChannel(998293017557487697);
            await channel.SendMessageAsync(embed: embed.Build(), components: buttons.Build());
            await RespondAsync($"Message successfully sent to <#{channel.Id}>", ephemeral: true);
        }
    }
}
