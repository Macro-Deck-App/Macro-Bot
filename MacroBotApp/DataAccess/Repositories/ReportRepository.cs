using AutoMapper;
using MacroBot.DataAccess.Entities;
using MacroBot.DataAccess.RepositoryInterfaces;
using MacroBot.Discord.Modules.Reports;
using Microsoft.EntityFrameworkCore;
using Serilog;
using ILogger = Serilog.ILogger;

namespace MacroBot.DataAccess.Repositories;

public class ReportRepository : IReportRepository
{
    private readonly MacroBotContext _dbContext;
    private readonly ILogger _logger = Log.ForContext<ReportRepository>();
    private readonly IMapper _mapper;

    public ReportRepository(MacroBotContext dbContext, IMapper mapper)
    {
        _dbContext = dbContext;
        _mapper = mapper;
    }

    public async Task<Report?> GetReport(string id)
    {
        var existingReport =
            await _dbContext.ReportEntities.FirstOrDefaultAsync(x => x.Id == id);

        if (existingReport is null)
        {
            _logger.Warning("Report {ReportId} not found", id);
            return null;
        }

        var mappedReport = _mapper.Map<ReportEntity, Report>(existingReport);
        return mappedReport;
    }

    public async Task<bool> ReportExists(string id)
    {
        return await _dbContext.ReportEntities.AnyAsync(x => x.Id == id);
    }

    public async Task<bool> ReportExists(ulong reporter, ulong user, ulong guildId)
    {
        return await _dbContext.ReportEntities.AnyAsync(x =>
            x.Reporter == reporter && x.User == user && x.Guild == guildId);
    }

    public async Task<string?> CreateReport(ulong reporter, ulong user, ulong guild,
        string content, ulong? channel = null, ulong? message = null, string? id = null,
        DateTime? reported = null)
    {

        var reportEntity = new ReportEntity
        {
            Reporter = reporter,
            User = user,
            Guild = guild,
            Id = id ?? GenerateReportId(6),
            Channel = channel,
            Message = message,
            Content = content,
            Reported = reported ?? DateTime.Now
        };
        await _dbContext.ReportEntities.AddAsync(reportEntity);
        await _dbContext.SaveChangesAsync();
        return reportEntity.Id;
    }

    public async Task DeleteReport(string id)
    {
        var existingReport = await _dbContext.ReportEntities.FirstOrDefaultAsync(x => x.Id == id);
        if (existingReport is null)
        {
            _logger.Warning("Cannot delete report {ReportId} - Report ID not found", id);
            return;
        }

        _dbContext.ReportEntities.Remove(existingReport);
        await _dbContext.SaveChangesAsync();
    }

    public async Task<IEnumerable<Report>> GetReportsForGuild(ulong guildId)
    {
        var guildReports = await _dbContext.ReportEntities.Where(x => x.Guild == guildId).ToArrayAsync();
        if (guildReports.Length == 0) return Enumerable.Empty<Report>();
        var guildReportsMapped = _mapper.Map<IEnumerable<ReportEntity>, IEnumerable<Report>>(guildReports);
        return guildReportsMapped;
    }

    public static string GenerateReportId(int length)
    {
        var random = new Random();
        const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789";
        return new string(Enumerable.Repeat(chars, length)
            .Select(s => s[random.Next(s.Length)]).ToArray());
    }
}