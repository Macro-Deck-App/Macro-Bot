using System.Text.Json.Serialization;
using MacroBot.Core.Enums;

namespace MacroBot.Core.Models.KoFi;

public class KoFiWebhookRequest
{
    [JsonPropertyName("verification_token")]
    public string? VerificationToken { get; set; }

    [JsonPropertyName("message_id")]
    public string? MessageId { get; set; }

    [JsonPropertyName("timestamp")]
    public DateTime Timestamp { get; set; }

    [JsonPropertyName("type"), JsonConverter(typeof(JsonStringEnumConverter))]
    public KoFiWebhookType? Type { get; set; }

    [JsonPropertyName("is_public")]
    public bool IsPublic { get; set; }

    [JsonPropertyName("from_name")]
    public string? FromName { get; set; }

    [JsonPropertyName("message")]
    public string? Message { get; set; }

    [JsonPropertyName("amount")]
    public string? Amount { get; set; }

    [JsonPropertyName("url")]
    public string? Url { get; set; }

    [JsonPropertyName("email")]
    public string? Email { get; set; }

    [JsonPropertyName("currency")]
    public string? Currency { get; set; }

    [JsonPropertyName("is_subscription_payment")]
    public bool IsSubscriptionPayment { get; set; }

    [JsonPropertyName("is_first_subscription_payment")]
    public bool IsFirstSubscriptionPayment { get; set; }

    [JsonPropertyName("kofi_transaction_id")]
    public string? KofiTransactionId { get; set; }

    [JsonPropertyName("shop_items")]
    public object? ShopItems { get; set; }

    [JsonPropertyName("tier_name")]
    public string? TierName { get; set; }

    [JsonPropertyName("shipping")]
    public object? Shipping { get; set; }
}