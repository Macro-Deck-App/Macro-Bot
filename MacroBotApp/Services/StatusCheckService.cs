using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using Discord.WebSocket;
using JetBrains.Annotations;
using MacroBot.Config;
using MacroBot.Models.Status;
using MacroBot.ServiceInterfaces;
using Serilog;
using ILogger = Serilog.ILogger;

namespace MacroBot.Services;

[UsedImplicitly]
public class StatusCheckService : IStatusCheckService, IHostedService
{
    private readonly ILogger _logger = Log.ForContext<StatusCheckService>();

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TimerService _timerService;
    private readonly StatusCheckConfig _statusCheckConfig;
    private readonly DiscordSocketClient _discordSocketClient;

    public event EventHandler<StatusCheckFinishedEventArgs>? StatusCheckFinished;
    public event EventHandler? StatusOfItemChanged;
    public event EventHandler? ItemStatusInCollectionChanged;

    public IEnumerable<StatusCheckResult>? LastStatusCheckResults { get; private set; }
    public DateTime LastStatusCheck { get; private set; }
    private bool _lastCheckDone = true;

    public StatusCheckService(IHttpClientFactory httpClientFactory,
        TimerService timerService,
        StatusCheckConfig statusCheckConfig,
        DiscordSocketClient discordSocketClient)
    {
        _httpClientFactory = httpClientFactory;
        _timerService = timerService;
        _statusCheckConfig = statusCheckConfig;
        _discordSocketClient = discordSocketClient;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timerService.FiveMinuteTimerElapsed += TimerServiceOnTimerElapsed;
        _discordSocketClient.Ready += DiscordSocketClientOnReady;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timerService.FiveMinuteTimerElapsed -= TimerServiceOnTimerElapsed;
        return Task.CompletedTask;
    }

    private async Task DiscordSocketClientOnReady()
    {
        await CheckAllAsync();
    }
    
    private async void TimerServiceOnTimerElapsed(object? sender, EventArgs e)
    {
        await CheckAllAsync();
    }

    private async Task CheckAllAsync()
    {
        if (!_lastCheckDone)
        {
            return;
        }
        _lastCheckDone = false;
        _logger.Verbose("Starting status check...");
        var results = new ConcurrentBag<StatusCheckResult>();
        var lastResults = LastStatusCheckResults?.ToArray();
        ParallelOptions parallelOptions = new()
        {
            MaxDegreeOfParallelism = 3
        };
        await Parallel.ForEachAsync(_statusCheckConfig.StatusCheckItems, parallelOptions, async (statusCheckItem, token) =>
        {
            var lastResult = lastResults?.FirstOrDefault(x => x.Name.Equals(statusCheckItem.Name));
            var result = await CheckAsync(statusCheckItem, lastResult);
            
            results.Add(result);
        });

        results = new ConcurrentBag<StatusCheckResult>(results.OrderByDescending(x => x.Name));

        _logger.Verbose(
            "Status check done. {NoOnline}/{Total} online {NoWarnings} with warnings",
            results.Count(x => x.Online),
            results.Count,
            results.Count(x => x.OnlineWithWarnings));

        LastStatusCheckResults = results.ToArray();
        LastStatusCheck = DateTime.Now;
        StatusCheckFinished?.Invoke(
            this, 
            new StatusCheckFinishedEventArgs
            {
                Results = LastStatusCheckResults
            });
        if (results.Any(x => x.StateChanged))
        {
            ItemStatusInCollectionChanged?.Invoke(this, EventArgs.Empty);
        }

        _lastCheckDone = true;
    }

    private async Task<StatusCheckResult> CheckAsync(StatusCheckConfig.StatusCheckItem statusCheckItem, StatusCheckResult? lastResult)
    {
        _logger.Verbose(
            "Checking status of {Name} - {Url}...", 
            statusCheckItem.Name, 
            statusCheckItem.Url);
        using var httpClient = _httpClientFactory.CreateClient();
        httpClient.Timeout = TimeSpan.FromSeconds(60);
        var onlineWithWarnings = false;
        bool online;
        HttpResponseMessage? request = null;
        try
        {
            request = await httpClient.GetAsync(statusCheckItem.Url);
            online = request.StatusCode == HttpStatusCode.OK;
        }
        catch
        {
            online = false;
        }
        var stateChanged = lastResult.HasValue && online != lastResult.Value.Online;
        var stateChangedAt = stateChanged ? DateTime.Now : lastResult?.StateChangedAt;
        
        if (!statusCheckItem.StatusEndpoint)
        {
            return new StatusCheckResult(statusCheckItem.Name,
                request?.StatusCode == HttpStatusCode.OK,
                request?.StatusCode ?? HttpStatusCode.NotFound,
                stateChanged, stateChangedAt);
        }

        try
        {
            if (request?.StatusCode == HttpStatusCode.OK)
            {
                online = true;
                var result = await request.Content.ReadAsStringAsync();
                var endpointResponse = JsonSerializer.Deserialize<StatusEndpointResponse>(result);
                onlineWithWarnings = endpointResponse is null || !endpointResponse.Ok;
            }
        }
        catch
        {
            online = false;
            onlineWithWarnings = request?.StatusCode == HttpStatusCode.OK;
        }

        stateChanged = online != lastResult?.Online && onlineWithWarnings != lastResult?.OnlineWithWarnings;
        stateChangedAt = stateChanged ? DateTime.Now : lastResult?.StateChangedAt;
        return new StatusCheckResult(statusCheckItem.Name,
            online,
            request?.StatusCode ?? HttpStatusCode.NotFound,
            stateChanged,
            stateChangedAt,
            onlineWithWarnings);
    }
}