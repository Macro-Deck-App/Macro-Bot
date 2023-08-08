using MacroBot.Core.Models.Webhook;
using Microsoft.AspNetCore.Http;

namespace MacroBot.Core.Authentication;

public static class WebhookAuthenticationHelper
{
    public static AuthenticationResult CheckAuthentication(this HttpRequest request, WebhookItem webhook)
    {
        var apiKey = $"Bearer {webhook.BearerAuthKey}";
        var authHeader = Enumerable.FirstOrDefault<string?>(request.Headers.Authorization);

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