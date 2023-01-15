using Discord;
using Discord.Interactions;
using MacroBot.Logging;
using MacroBot.Services;
using MacroBot.Utils;

namespace MacroBot.Commands.Tickets;

public class TicketDescriptionSubmit : InteractionModuleBase<SocketInteractionContext>
{
    [ModalInteraction("ticket_create|wrong_expectation|reason")]
    public async Task CreateExpectation(Modal modal)
    {
        var embed = await TicketCreateButton.CreateEmbed(Context.Client);
        embed.WithTitle("Expected something different");
        embed.WithDescription("We're sorry, that you expected something different from Macro Deck.");
        embed.AddField("Your Description", Context.Interaction.Data, false);

        ComponentBuilder buttons = new();
        buttons.AddRow(await TicketCreateButton.CreateActionRow());

        await TicketCreateButton.CreateChannelAsync(Context.Guild, Context.User, $"{Context.User.Mention} your ticket is created here and a <@&{ConfigManager.GlobalConfig.Roles.SupportRoleId}> will help you soon:", embed.Build(), buttons.Build());
        await RespondAsync("Your Ticket is created!", ephemeral: true);
    }

    [ModalInteraction("ticket_create|other|reason")]
    public async Task CreateOther(Modal modal)
    {
        Logger.Debug(Modules.Tickets, "Modal received!");
        var embed = await TicketCreateButton.CreateEmbed(Context.Client);
        embed.WithTitle("Some other problems");
        embed.WithDescription("We're sorry, that you are facing problems with Macro Deck.");
        embed.AddField("Your Description", Context.Interaction.Data, false);

        ComponentBuilder buttons = new();
        buttons.AddRow(await TicketCreateButton.CreateActionRow());

        await TicketCreateButton.CreateChannelAsync(Context.Guild, Context.User, $"{Context.User.Mention} your ticket is created here and a <@&{ConfigManager.GlobalConfig.Roles.SupportRoleId}> will help you soon:", embed.Build(), buttons.Build());
        await RespondAsync("Your Ticket is created!", ephemeral: true);
    }
}