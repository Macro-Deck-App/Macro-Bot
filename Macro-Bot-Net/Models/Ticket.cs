namespace Develeon64.MacroBot.Models {
	public class Ticket {
		public ulong Author { get; set; }
		public ulong Channel { get; set; }
		public ulong Message { get; set; }
		public DateTime Created { get; set; }
		public DateTime Modified { get; set; }
	}
}
