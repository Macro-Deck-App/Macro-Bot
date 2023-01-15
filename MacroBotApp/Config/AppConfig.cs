namespace MacroBot.Config;

public class BotConfig : LoadableConfig<BotConfig>
{
	public string Token { get; set; }
	public string GithubToken { get; set; }
	public ulong TestGuildId { get; set; }
	public ulong BotId { get; set; }
	public ulong ProductiveGuildId { get; set; }
	public RolesConfig Roles { get; set; } = new();
	public ChannelsConfig Channels { get; set; } = new();
	
	public class RolesConfig {
		public ulong ModeratorRoleId { get; set; }
		public ulong SupportRoleId { get; set; }
	}

	public class ChannelsConfig {
		public ulong MemberScreeningChannelId { get; set; }
		public ulong SupportTeamChannelId { get; set; }
		public ulong UpdateChannelId { get; set; }
		public ulong FaqChannelId { get; set; }
		public ulong[] ImageOnlyChannels { get; set; }
	}
}

