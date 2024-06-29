using OrchardCore.Modules;
using WebhooksCore;

namespace OrchardExperiments.Webhooks.Handlers;

public class StartWebhooksProcessor(IBackgroundTaskProcessor backgroundTaskProcessor) : ModularTenantEvents
{
    public override async Task ActivatedAsync()
    {
        await backgroundTaskProcessor.StartAsync();
    }

    public override async Task TerminatingAsync()
    {
        await backgroundTaskProcessor.StopAsync();
    }
}