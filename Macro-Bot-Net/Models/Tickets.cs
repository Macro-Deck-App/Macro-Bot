using SQLite;

namespace Develeon64.MacroBot.Models {
	public class Tickets {
		[PrimaryKey]
		public String Author { get; set; }
		public String Channel { get; set; }
		public String Message { get; set; }
		public DateTime Created { get; set; }
		public DateTime Modified { get; set; }
	}
}
