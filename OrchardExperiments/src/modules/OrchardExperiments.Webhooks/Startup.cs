using Microsoft.Extensions.DependencyInjection;
using OrchardCore.ContentManagement.Handlers;
using OrchardCore.Modules;
using OrchardExperiments.Webhooks.Handlers;
using OrchardExperiments.Webhooks.Providers;
using WebhooksCore;
using WebhooksCore.Options;

namespace OrchardExperiments.Webhooks;

public class Startup : StartupBase
{
    public override void ConfigureServices(IServiceCollection services)
    {
        services.Configure<WebhookBroadcasterOptions>(options => options.UseBackgroundProcessorBroadcasterStrategy());
        
        services
            .AddWebhooksCore()
            .AddSingleton<IWebhookSinkProvider, ElsaServerWebhookSinkProvider>()
            .AddScoped<IContentHandler, ContentEventHandlers>()
            .AddScoped<IModularTenantEvents, StartWebhooksProcessor>()
            ;
    }
}