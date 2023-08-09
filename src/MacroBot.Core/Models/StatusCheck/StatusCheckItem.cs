namespace MacroBot.Core.Models.StatusCheck;

public class StatusCheckItem
{
    public string Name { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public bool StatusEndpoint { get; set; }
}