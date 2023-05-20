using System.Text.Json;
using MacroBot.Config;
using MacroBot.Crypto;
using MacroBot.ManagerInterfaces;
using MacroBot.Models.KoFi;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace MacroBot.Controllers;

[Route("/kofi")]
public class KoFiController : ControllerBase
{
	private readonly KoFiConfig _koFiConfig;
	private readonly IKoFiManager _koFiManager;

	public KoFiController(KoFiConfig koFiConfig,
		IKoFiManager koFiManager)
	{
		_koFiConfig = koFiConfig;
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

		if (!verificationToken.EqualsCryptographically(_koFiConfig.VerificationToken))
		{
			return new StatusCodeResult(StatusCodes.Status403Forbidden);
		}

		koFiWebhookRequest.VerificationToken = null;

		await _koFiManager.HandleWebhook(koFiWebhookRequest);
		return Ok();
	}
}