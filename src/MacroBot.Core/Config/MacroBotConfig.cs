using MacroBot.Core.Models.StatusCheck;
using MacroBot.Core.Models.Webhook;

namespace MacroBot.Core.Config;

public static partial class MacroBotConfig
{
    public static string DatabaseConnectionString => GetString("database:connection_string");

    public static string ExtensionStoreApiUrl => GetString("extension_store_api_url");
    
    public static string BotToken => GetString("bot:token");
    public static ulong GuildId => GetUlong("bot:guild_id");
    
    public static ulong ModeratorRoleId => GetUlong("roles:moderator_role_id");
    public static ulong AdministratorRoleId => GetUlong("roles:moderator_role_id");

    public static ulong LogChannelId => GetUlong("channels:log_channel_id");
    public static ulong ErrorLogChannelId => GetUlong("channels:error_log_channel_id");
    public static ulong MemberScreeningChannelId => GetUlong("channels:member_screening_channel_id");
    public static ulong[] ImageOnlyChannelIds => GetUlongArray("channels:image_only_channel_ids");

    public static ulong[] PermissionManageTags => GetUlongArray("permissions:manage_tags");

    public static string KoFiVerificationToken => GetString("kofi:verification_token");
    public static ulong KoFiDonationChannelId => GetUlong("kofi:donation_channel_id");

    public static WebhookItem[] Webhooks => GetWebhookItemArray("webhooks");
    
    public static ulong StatusCheckChannelId => GetUlong("status_check:channel_id");
    public static StatusCheckItem[] StatusCheckItems => GetStatusCheckItemArray("status_check:items");
}