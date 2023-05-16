namespace MacroBot.Config;

public class ExtensionDetectionConfig : LoadableConfig<ExtensionDetectionConfig>
{
    public string AllExtensionsUrl { get; set; } =
        "https://test.extensionstore.api.macro-deck.app/rest/v2/extensions";

    public string SearchExtensionsUrl { get; set; } =
        "https://test.extensionstore.api.macro-deck.app/rest/v2/extensions/search";

    public int ExtensionsPerPage { get; set; } = 100;
}