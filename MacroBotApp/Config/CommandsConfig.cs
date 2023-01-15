namespace MacroBot.Config;

public class CommandsConfig : LoadableConfig<CommandsConfig>
{
	public TaggingConfig Tagging { get; set; } = new();
	
	public class TaggingConfig
	{
		public ulong[] PermissionManageTags { get; set; }
	}
}

