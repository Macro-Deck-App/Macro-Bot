using Develeon64.MacroBot.Services;
using Develeon64.MacroBot.Utils;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;

namespace Develeon64.MacroBot.Commands.Tickets
{
    public class TicketActionButton : InteractionModuleBase<SocketInteractionContext>
    {
        [ComponentInteraction("ticket_action|close_ticket")]
        public async Task CloseTicket()
        {
            await (Context.Channel as SocketTextChannel).DeleteAsync();
            await DatabaseManager.DeleteTicket(Context.Channel.Id, IdType.Channel);

            await RespondAsync("Your ticket is now closed.", ephemeral: true);
        }

        [ComponentInteraction("ticket_action|more_help")]
        public async Task MoreHelp()
        {
            await Context.Guild.GetTextChannel(ConfigManager.GlobalConfig.Channels.SupportTeamChannelId).SendMessageAsync($"{Context.User.Mention} ({Context.User.Username}) needs the help of a <@&{ConfigManager.GlobalConfig.Roles.SupportRoleId}> in <#{Context.Channel.Id}>!");

            ActionRowComponent oldRow = (Context.Interaction as SocketMessageComponent).Message.Components.ElementAt(0);
            ActionRowBuilder row = new();
            foreach (ButtonComponent button in oldRow.Components)
            {
                ButtonBuilder builder = button.ToBuilder();
                if (button.CustomId.Contains("more_help"))
                    builder.WithDisabled(true);
                row.WithButton(builder);
            }

            await (Context.Interaction as SocketMessageComponent).Message.ModifyAsync((message) =>
            {
                message.Components = new ComponentBuilder().AddRow(row).Build();
            });
            await RespondAsync($"The <@&{ConfigManager.GlobalConfig.Roles.SupportRoleId}>-Team has been contacted.");
        }

        [ComponentInteraction("ticket_action|plugin_select")]
        public async Task CreatePluginProblems()
        {
            string[] id = (Context.Interaction as SocketMessageComponent).Data.Values.ElementAt(0).Split('.');
            string author = id[0];
            string plugin = id[1];

            ulong authorID = await DatabaseManager.GetPluginAuthorId(plugin);

            DiscordEmbedBuilder embed = await TicketCreateButton.CreateEmbed(Context.Client);
            embed.WithDescription("The creator of the plugin will help you soon.");

            switch (plugin.ToLower())
            {
                case "twitchplugin":
                    embed.WithTitle("Twitch-Problems");
                    break;
                case "windowsutils":
                    embed.WithTitle("Problems with Windows Utils");
                    break;
                case "spotifyplugin":
                    embed.WithTitle("Spotify-Problems");
                    break;
                case "magichome":
                    embed.WithTitle("Problems with Magic Home");
                    break;
            }

            ComponentBuilder buttons = new();
            buttons.AddRow(await TicketCreateButton.CreateActionRow());

            await (Context.Interaction as SocketMessageComponent).Message.ModifyAsync((m) =>
            {
                m.Embed = embed.Build();
                m.Components = buttons.Build();
            });
            await RespondAsync($"Your ticket is updated, please wait for <@{authorID}> to answer your ticket.");
        }
    }
}
