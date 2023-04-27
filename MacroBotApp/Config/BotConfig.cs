namespace MacroBot.Config;

public class BotConfig : LoadableConfig<BotConfig>
{
	public string Token { get; set; }
	public ulong GuildId { get; set; }
	public RolesConfig Roles { get; set; } = new();
	public ChannelsConfig Channels { get; set; } = new();
	
	public class RolesConfig
	{
		public ulong ModeratorRoleId { get; set; }
		public ulong AdministratorRoleId { get; set; }
	}

	public class ChannelsConfig
	{
		public ulong LogChannelId { get; set; }
		public ulong ErrorLogChannelId { get; set; }
		public ulong MemberScreeningChannelId { get; set; }
		public ulong StatusCheckChannelId { get; set; }
		public ulong[] ImageOnlyChannels { get; set; }
	}
}

