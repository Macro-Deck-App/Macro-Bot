using System.Text.Json;
using MacroBot.Core.Config;
using MacroBot.Core.Crypto;
using MacroBot.Core.ManagerInterfaces;
using MacroBot.Core.Models.KoFi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MacroBot.Controllers;

[Route("/kofi")]
public class KoFiController : ControllerBase
{
    private readonly IKoFiManager _koFiManager;

    public KoFiController(
        IKoFiManager koFiManager)
    {
        _koFiManager = koFiManager;
    }

    [HttpPost("webhook")]
    [AllowAnonymous]
    [Consumes("application/x-www-form-urlencoded")]
    public async Task<IActionResult> DonationCreated([FromForm] string data)
    {
        var koFiWebhookRequest = JsonSerializer.Deserialize<KoFiWebhookRequest>(data);
        if (koFiWebhookRequest == null)
        {
            return new BadRequestResult();
        }
        var verificationToken = koFiWebhookRequest.VerificationToken;
        if (string.IsNullOrWhiteSpace(verificationToken))
        {
            return new UnauthorizedResult();
        }

        if (!verificationToken.EqualsCryptographically(MacroBotConfig.KoFiVerificationToken))
        {
            return new StatusCodeResult(StatusCodes.Status403Forbidden);
        }

        koFiWebhookRequest.VerificationToken = null;

        await _koFiManager.HandleWebhook(koFiWebhookRequest);
        return Ok();
    }
}