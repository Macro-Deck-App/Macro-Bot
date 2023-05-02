namespace MacroBot.Config;

public class BuyMeACoffeeConfig : LoadableConfig<BuyMeACoffeeConfig>
{
    public string SignatureSecret { get; set; } = string.Empty;
    public ulong ChannelId { get; set; }
}