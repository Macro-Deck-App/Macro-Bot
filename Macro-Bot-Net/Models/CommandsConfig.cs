using Newtonsoft.Json.Linq;

namespace Develeon64.MacroBot.Models {
	public class CommandsConfig {
		public TaggingConfig Tagging { get; set; }
		public PollsConfig Polls { get; set; }
	}

	public class TaggingConfig
    {
		public ulong[] PermissionManageTags { get; set; }

	}

	public class PollsConfig
    {
		public ulong[] PermissionManagePolls { get; set; }
    }
}
