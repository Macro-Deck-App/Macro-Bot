using MacroBot.Config;
using MacroBot.Crypto;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace MacroBot.Authentication;

public class BuyMeACoffeeWebhookAttribute : Attribute, IAsyncAuthorizationFilter
{
    private const string SignatureHeader = "x-signature-sha256";

    public async Task OnAuthorizationAsync(AuthorizationFilterContext context)
    {
        if (!context.HttpContext.Request.Headers.TryGetValue(SignatureHeader, out var headerSignature))
        {
            context.Result = new UnauthorizedResult();
        }

        context.HttpContext.Request.Headers.Remove(SignatureHeader);

        var buyMeACoffeeConfig = context.HttpContext.RequestServices.GetRequiredService<BuyMeACoffeeConfig>();

        var computedSignature =
            await Signature.GenerateSignatureFromRequest(context.HttpContext.Request,
                buyMeACoffeeConfig.SignatureSecret);
        
        var signatureMatch = Signature.VerifySignature(headerSignature, computedSignature);
        if (!signatureMatch)
        {
            context.Result = new StatusCodeResult(StatusCodes.Status403Forbidden);
        }
    }
}