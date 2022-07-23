namespace Develeon64.MacroBot.Models {
	public class Poll {
		public long Id { get; set; }
		public ulong Author { get; set; }
		public ulong MessageId { get; set; }
		public ulong ChannelId { get; set; }
		public ulong GuildId { get; set; }
		public string Name { get; set; }
		public string Description { get; set; }
		public bool Closed { get; set; }

		public int Votes1 { get; set; }
		public int Votes2 { get; set; }
		public int Votes3 { get; set; }
		public int Votes4 { get; set; }
	}
}
