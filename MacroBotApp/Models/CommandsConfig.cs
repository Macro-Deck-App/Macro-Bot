namespace MacroBot.Models;

public class CommandsConfig {
	public TaggingConfig Tagging { get; set; }
}

public class TaggingConfig
{
	public ulong[] PermissionManageTags { get; set; }

}