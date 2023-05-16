using System.Net;

namespace MacroBot.Models.Status;

public struct StatusCheckResult
{
    public string Name { get; }
    public bool Online { get; }
    public HttpStatusCode StatusCode { get; }
    public bool OnlineWithWarnings { get; }
    public bool StateChanged { get; }
    public DateTime? StateChangedAt { get; }

    public StatusCheckResult(string name, bool online, HttpStatusCode statusCode, bool stateChanged,
        DateTime? stateChangedAt, bool onlineWithWarnings = false)
    {
        Name = name;
        Online = online;
        StatusCode = statusCode;
        StateChanged = stateChanged;
        StateChangedAt = stateChangedAt;
        OnlineWithWarnings = onlineWithWarnings;
    }
}