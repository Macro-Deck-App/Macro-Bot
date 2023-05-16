namespace MacroBot.Config;

public class CommandsConfig : LoadableConfig<CommandsConfig>
{
    public TaggingConfig Tagging { get; set; } = new();

    public TranslateConfig Translate { get; set; } = new();

    public class TaggingConfig
    {
        public ulong[] PermissionManageTags { get; set; }
    }

    public class TranslateConfig
    {
        public string UserName { get; set; }
        public string Password { get; set; }
        public string Url { get; set; }
    }
}