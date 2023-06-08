using MacroBot.Discord.Modules.Reports;

namespace MacroBot.DataAccess.RepositoryInterfaces;

public interface IReportRepository
{
    public Task<Report?> GetReport(string id);
    public Task<IEnumerable<Report>> GetReportsForGuild(ulong guildId);
    public Task DeleteReport(string id);

    public Task<string?> CreateReport(ulong reporter, ulong user, ulong guild,
        string content, ulong? channel = null, ulong? message = null, string? id = null,
        DateTime? reported = null);

    public Task<bool> ReportExists(ulong reporter, ulong user, ulong guildId);
    public Task<bool> ReportExists(string id);
}