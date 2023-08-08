namespace MacroBot.Core.Config;

public class StatusCheckConfig : LoadableConfig<StatusCheckConfig>
{
    public List<StatusCheckItem> StatusCheckItems { get; set; } = new()
    {
        new StatusCheckItem()
        {
            Name = "Website",
            Url = "https://macro-deck.app"
        }
    };
    
    public class StatusCheckItem
    {
        public string Name { get; set; }
        public string Url { get; set; }
        public bool StatusEndpoint { get; set; }
    }
}