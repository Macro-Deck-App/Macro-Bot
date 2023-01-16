using System.Collections.Concurrent;
using System.Net;
using System.Text.Json;
using MacroBot.Config;
using MacroBot.Models;
using MacroBot.ServiceInterfaces;
using Microsoft.Extensions.Hosting;
using Serilog;

namespace MacroBot.Services;

public class StatusCheckService : IStatusCheckService, IHostedService
{
    private readonly ILogger _logger = Log.ForContext<StatusCheckService>();

    private readonly IHttpClientFactory _httpClientFactory;
    private readonly TimerService _timerService;
    private readonly StatusCheckConfig _statusCheckConfig;
    
    public event EventHandler<StatusCheckFinishedEventArgs>? StatusCheckFinished;
    public event EventHandler? StatusOfItemChanged;
    public event EventHandler? ItemStatusInCollectionChanged;

    public IEnumerable<StatusCheckResult> LastStatusCheckResults { get; private set; } = Enumerable.Empty<StatusCheckResult>();
    public DateTime LastStatusCheck { get; private set; }

    public StatusCheckService(IHttpClientFactory httpClientFactory, TimerService timerService, StatusCheckConfig statusCheckConfig)
    {
        _httpClientFactory = httpClientFactory;
        _timerService = timerService;
        _statusCheckConfig = statusCheckConfig;
    }

    public Task StartAsync(CancellationToken cancellationToken)
    {
        _timerService.MinuteTimerElapsed += TimerServiceOnMinuteTimerElapsed;
        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        _timerService.MinuteTimerElapsed -= TimerServiceOnMinuteTimerElapsed;
        return Task.CompletedTask;
    }

    private async void TimerServiceOnMinuteTimerElapsed(object? sender, EventArgs e)
    {
        await CheckAllAsync();
    }

    private async Task CheckAllAsync()
    {
        _logger.Information("Starting status check...");
        var results = new ConcurrentBag<StatusCheckResult>();
        var statusInCollectionChanged = false;
        foreach (var statusCheckItem in _statusCheckConfig.StatusCheckItems)
        {
            var result = await CheckAsync(statusCheckItem);
            var lastResult = LastStatusCheckResults.FirstOrDefault(x => x.Name.Equals(statusCheckItem.Name));
            if (lastResult.Online != result.Online ||
                lastResult.OnlineWithWarnings != result.OnlineWithWarnings ||
                lastResult.StatusCode != result.StatusCode)
            {
                statusInCollectionChanged = true;
                StatusOfItemChanged?.Invoke(result, EventArgs.Empty);
            }
            results.Add(result);
        }

        _logger.Information(
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
        if (statusInCollectionChanged)
        {
            ItemStatusInCollectionChanged?.Invoke(this, EventArgs.Empty);
        }
    }

    private async Task<StatusCheckResult> CheckAsync(StatusCheckConfig.StatusCheckItem statusCheckItem)
    {
        _logger.Information(
            "Checking status of {Name} - {Url}...", 
            statusCheckItem.Name, 
            statusCheckItem.Url);
        using var httpClient = _httpClientFactory.CreateClient();
        var request = await httpClient.GetAsync(statusCheckItem.Url);
        
        if (!statusCheckItem.StatusEndpoint)
        {
            return new StatusCheckResult(statusCheckItem.Name, request.StatusCode == HttpStatusCode.OK, request.StatusCode);
        }

        var onlineWithWarnings = false;
        var online = false;
        try
        {
            if (request.StatusCode == HttpStatusCode.OK)
            {
                online = true;
                var result = await request.Content.ReadAsStringAsync();
                var endpointResponse = JsonSerializer.Deserialize<StatusEndpointResponse>(result);
                onlineWithWarnings = endpointResponse is null || !endpointResponse.Ok;
            }
        }
        catch (Exception ex)
        {
            onlineWithWarnings = request.StatusCode == HttpStatusCode.OK;
        }
        return new StatusCheckResult(statusCheckItem.Name, online, request.StatusCode, onlineWithWarnings);
    }
}