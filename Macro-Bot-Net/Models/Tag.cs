namespace Develeon64.MacroBot.Models {
	public class Tag {
		public ulong Author { get; set; }
		public ulong Guild { get; set; }
		public string Name { get; set; }
		public string Content { get; set; }

		public ulong LastEditor { get; set; }
		public DateTime LastEdited { get; set; }
	}
}
