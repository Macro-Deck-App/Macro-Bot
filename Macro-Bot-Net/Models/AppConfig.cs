namespace Develeon64.MacroBot.Models {
	public class AppConfig {
		public string Token { get; set; }
		public string GithubToken { get; set; }
		public ulong TestGuildId { get; set; }
		public ulong BotId { get; set; }
		public ulong ProductiveGuildId { get; set; }
		public RolesConfig Roles { get; set; }
		public ChannelsConfig Channels { get; set; }
	}

	public class RolesConfig {
		public ulong AdministratorRoleId { get; set; }
		public ulong ModeratorRoleId { get; set; }
		public ulong SupportRoleId { get; set; }
	}

	public class ChannelsConfig {
		public ulong MemberScreeningChannelId { get; set; }
		public ulong TicketCategoryId { get; set; }
		public ulong SupportTeamChannelId { get; set; }
		public ulong UpdateChannelId { get; set; }
		public ulong FaqChannelId { get; set; }
		public ulong[] ImageOnlyChannels { get; set; }
	}
}
