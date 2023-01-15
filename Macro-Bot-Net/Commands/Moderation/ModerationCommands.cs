using Develeon64.MacroBot.Utils;
using Develeon64.MacroBot.Models;
using Develeon64.MacroBot.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using System.Reflection;
using Develeon64.MacroBot.Commands.Tagging;

namespace Develeon64.MacroBot.Commands
{
    public class ModerationCommands : InteractionModuleBase<SocketInteractionContext>
    {
        [SlashCommand("timeout", "Timeout a person")]
        [RequireUserPermission(GuildPermission.ModerateMembers)]
        [RequireBotPermission(GuildPermission.ModerateMembers)]
        public async Task TimeoutAsync([Summary(description: "The user you want to timeout")] IUser user, int days = 0, int hours = 1, int minutes = 0, int seconds = 0)
        {
            var modal = new ModalBuilder()
                .WithTitle($"Timeout {user.Username}#{user.Discriminator}?")
                .WithCustomId("timeout-modal")
                .AddTextInput("Reason of the timeout?", "timeout-reason", TextInputStyle.Paragraph, "Write your reason here.", 1, 512, true);

            Context.Client.ModalSubmitted += async modal =>
            {
                var until = new TimeSpan(days, hours, minutes, seconds);

                // Get the values of components.
                List<SocketMessageComponentData> components =
                    modal.Data.Components.ToList();
                string reason = components
                    .First(x => x.CustomId == "timeout-reason").Value;

                // Send a message to that user that he is timed out.
                DiscordEmbedBuilder embed = new()
                {
                    Title = $"You are muted on {Context.Guild.Name}!",
                    Description = $"You cannot appeal a mute! Just wait until your mute is finished."
                };
                embed.AddField("Reason", reason);
                embed.AddField("Until", $"{until.ToString(@"dd\.hh\:mm\:ss")}");

                var mesg = await user.SendMessageAsync(embed: embed.Build());

                // Timeout the user.
                try {
                    await ModerationUtils.TimeoutAsync(user, Context.Guild, until);
                }
                catch (Exception e) {
                    await mesg.DeleteAsync();
                    await modal.RespondAsync($"An error occured./r/n```{e}```", ephemeral: true);
                }

                // Create a embed.
                embed = new()
                {
                    Title = $"User {user.Username}#{user.Discriminator} is timed out.",
                };
                embed.AddField("Reason", reason);
                embed.AddField("Until", $"{until.ToString(@"dd\.hh\:mm\:ss")}");

                // Respond to the modal.
                await modal.RespondAsync(embed: embed.Build(), ephemeral: true);
            };

            await RespondWithModalAsync(modal.Build());
        }

        [RequireUserPermission(GuildPermission.BanMembers)]
        [RequireUserPermission(GuildPermission.BanMembers)]
        [UserCommand("Ban this user")]
        public async Task BanUserCommandAsync(IUser user)
        {
            await BanAsync(user);
        }

        [SlashCommand("ban", "Ban a person")]
        public async Task BanAsync([Summary(description:"The user you want to ban")]IUser user, [Summary(description:"Prune the specified number of days (def: 0)")]int prune = 0)
        {
            var modal = new ModalBuilder()
                .WithTitle($"Ban {user.Username}#{user.Discriminator}?")
                .WithCustomId("ban-modal")
                .AddTextInput("Reason of the ban?", "ban-reason", TextInputStyle.Paragraph, "Write your reason here.", 1, 512, true);

            Context.Client.ModalSubmitted += async modal =>
            {
                // Get the values of components.
                List<SocketMessageComponentData> components =
                    modal.Data.Components.ToList();
                string reason = components
                    .First(x => x.CustomId == "ban-reason").Value;

                // Send a message to that user that he is banned.
                DiscordEmbedBuilder embed = new()
                {
                    Title = $"You are banned on {Context.Guild.Name}!",
                    Description = $"To appeal, contact suchbyte@gmail.com."
                };
                embed.AddField("Reason", reason);

                await user.SendMessageAsync(embed: embed.Build());

                // Ban the user.
                await Context.Guild.GetUser(user.Id).BanAsync(prune, reason);

                // Create a embed.
                embed = new()
                {
                    Title = $"User {user.Username}#{user.Discriminator} is banned.",
                };
                embed.AddField("Reason", reason);

                // Respond to the modal.
                await modal.RespondAsync(embed: embed.Build(), ephemeral: true);
            };

            await RespondWithModalAsync(modal.Build());
        }
    }
}
