using MacroBot.Core.Models.KoFi;

namespace MacroBot.Core.ManagerInterfaces;

public interface IKoFiManager
{
    public Task HandleWebhook(KoFiWebhookRequest koFiWebhookRequest);
}