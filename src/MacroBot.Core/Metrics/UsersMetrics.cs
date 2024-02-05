using System.Diagnostics.Metrics;

namespace MacroBot.Core.Metrics;

public class UsersMetrics
{
    private readonly Counter<int> _usersCounter;

    public UsersMetrics(IMeterFactory meterFactory)
    {
        var meter = meterFactory.Create("MacroBot");
        _usersCounter = meter.CreateCounter<int>("macrobot.users");
    }

    public void UserJoined()
    {
        _usersCounter.Add(1);
    }

    public void UserLeft()
    {
        _usersCounter.Add(-1);
    }
}