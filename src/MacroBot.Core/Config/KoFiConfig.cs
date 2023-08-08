namespace MacroBot.Core.Config;

public class KoFiConfig : LoadableConfig<KoFiConfig>
{
    public string VerificationToken { get; set; } = string.Empty;
    public ulong ChannelId { get; set; }
}