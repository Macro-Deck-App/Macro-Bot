using MacroBot.Models.Webhook;

namespace MacroBot.Authentication;

public static class WebhookAuthenticationHelper
{
    public static AuthenticationResult CheckAuthentication(this HttpRequest request, WebhookItem webhook)
    {
        var apiKey = $"Bearer {webhook.BearerAuthKey}";
        var authHeader = request.Headers.Authorization.FirstOrDefault();

        if (string.IsNullOrEmpty(authHeader))
        {
            return AuthenticationResult.Unauthorized;
        }

        if (!apiKey.Equals(authHeader, StringComparison.CurrentCulture))
        {
            return AuthenticationResult.Forbidden;
        }

        return AuthenticationResult.Authorized;
    }
}