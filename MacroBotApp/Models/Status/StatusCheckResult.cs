using System.Net;

namespace MacroBot.Models.Status;

public struct StatusCheckResult
{
    public string Name { get; }
    public bool Online { get; }
    public HttpStatusCode StatusCode { get; }
    public bool OnlineWithWarnings { get; }

    public StatusCheckResult(string name, bool online, HttpStatusCode statusCode, bool onlineWithWarnings = false)
    {
        Name = name;
        Online = online;
        StatusCode = statusCode;
        OnlineWithWarnings = onlineWithWarnings;
    }
}