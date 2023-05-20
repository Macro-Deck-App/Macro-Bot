using MacroBot.Models.KoFi;

namespace MacroBot.ManagerInterfaces;

public interface IKoFiManager
{
	public Task HandleWebhook(KoFiWebhookRequest koFiWebhookRequest);
}