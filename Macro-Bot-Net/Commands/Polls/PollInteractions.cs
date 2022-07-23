using Develeon64.MacroBot.Logging;
using Develeon64.MacroBot.Models;
using Develeon64.MacroBot.Services;
using Discord;
using Discord.Interactions;
using Discord.WebSocket;
using Newtonsoft.Json.Linq;

namespace Develeon64.MacroBot.Commands.Polls
{
    public class PollInteractions : InteractionModuleBase<SocketInteractionContext>
    {

        // Modals
        [ModalInteraction("poll_create_modal")]
        public async Task PollCreateModalHandler(PollCreateModal modal)
        {
            // Handle Assignables
            PollChannelAssignable assignable = PollCommands.createAssignables.Find(x => x.user == Context.User.Id);
            if (assignable == null)
            {
                await RespondAsync(PollUtils.getTagInfoError(), ephemeral: true);
                return;
            }

            // Handle Channel
            ITextChannel channel = (ITextChannel)Context.Channel;
            if (assignable.channel != null) channel = assignable.channel;

            // Handle Option
            PollOption option = assignable.option;

            // Remove Assignables
            PollCommands.createAssignables.Remove(assignable);

            // Build Embed
            EmbedBuilder embed = new EmbedBuilder()
            {
                Title = modal.Name,
                Description = modal.Description,
            };

            // Build Components
            ComponentBuilder componentBuilder = new();

            switch (option)
            {
                case PollOption.YesNo:
                    componentBuilder.WithButton("Yes", "poll_button_yes", ButtonStyle.Success);
                    componentBuilder.WithButton("No", "poll_button_no", ButtonStyle.Danger);

                    break;
                case PollOption.OneTwo:
                    componentBuilder.WithButton(" ","poll_button_one",ButtonStyle.Primary,new Emoji("1️⃣"));
                    componentBuilder.WithButton(" ", "poll_button_two", ButtonStyle.Primary, new Emoji("2️⃣"));

                    break;
                case PollOption.OneTwoThree:
                    componentBuilder.WithButton(" ", "poll_button_one", ButtonStyle.Primary, new Emoji("1️⃣"));
                    componentBuilder.WithButton(" ", "poll_button_two", ButtonStyle.Primary, new Emoji("2️⃣"));
                    componentBuilder.WithButton(" ", "poll_button_three", ButtonStyle.Primary, new Emoji("3️⃣"));

                    break;
                case PollOption.OneTwoThreeFour:
                    componentBuilder.WithButton(" ", "poll_button_one", ButtonStyle.Primary, new Emoji("1️⃣"));
                    componentBuilder.WithButton(" ", "poll_button_two", ButtonStyle.Primary, new Emoji("2️⃣"));
                    componentBuilder.WithButton(" ", "poll_button_three", ButtonStyle.Primary, new Emoji("3️⃣"));
                    componentBuilder.WithButton(" ", "poll_button_four", ButtonStyle.Primary, new Emoji("4️⃣"));

                    break;
                default:
                    break;
            }

            // Send messages
            IUserMessage msg = await channel.SendMessageAsync(embed: embed.Build(), components: componentBuilder.Build());

            long pollId = await DatabaseManager.CreatePoll(Context.User.Id,msg.Id, Context.Channel.Id, Context.Guild.Id, modal.Name, modal.Description, option);

            await RespondAsync($"Poll (ID: {pollId}) has been sent to <#{channel.Id}>", ephemeral: true);
        }


        // Buttons
        [ComponentInteraction("poll_button_yes")]
        public async Task PollButtonYesInteraction()
        {
            SocketUserMessage message = (Context.Interaction as SocketMessageComponent).Message;
            Poll poll = await DatabaseManager.GetPoll(message.Id);
            if (poll.Voted.FirstOrDefault(id => id.ToObject<ulong>() == Context.Interaction.User.Id) != null)
            {
                await RespondAsync(PollUtils.getAlreadyVotedError(), components: new ComponentBuilder().WithButton("Remove my Vote", "poll_button_remove_vote", ButtonStyle.Danger).Build(),ephemeral: true);
                return;
            }
        }
        [ComponentInteraction("poll_button_no")]
        public async Task PollButtonNoInteraction()
        {

        }
        [ComponentInteraction("poll_button_one")]
        public async Task PollButtonOneInteraction()
        {

        }
        [ComponentInteraction("poll_button_two")]
        public async Task PollButtonTwoInteraction()
        {

        }
        [ComponentInteraction("poll_button_three")]
        public async Task PollButtonThreeInteraction()
        {

        }
        [ComponentInteraction("poll_button_four")]
        public async Task PollButtonFourInteraction()
        {

        }
    }

    public class PollCreateModal : IModal
    {
        public string Title => "Create new Poll";
        [InputLabel("Poll Name")]
        [ModalTextInput("poll_create_name", TextInputStyle.Short, minLength: 1, maxLength: 100)]
        public string Name { get; set; }
        
        [InputLabel("Poll Description")]
        [ModalTextInput("poll_create_desc", TextInputStyle.Paragraph, minLength: 1, maxLength: 1000)]
        public string Description { get; set; }
    }
}
