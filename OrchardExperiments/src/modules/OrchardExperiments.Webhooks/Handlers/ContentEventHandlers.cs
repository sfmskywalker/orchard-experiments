using OrchardCore.ContentManagement.Handlers;
using OrchardExperiments.Webhooks.Payloads;
using WebhooksCore;

namespace OrchardExperiments.Webhooks.Handlers;

public class ContentEventHandlers(IWebhookEventBroadcaster webhookEventBroadcaster) : ContentHandlerBase
{
    public override async Task CreatedAsync(CreateContentContext context)
    {
        var payload = ContentItemEventPayload.Create(context.ContentItem);
        await webhookEventBroadcaster.BroadcastAsync(EventTypes.ContentItem.Created, payload);
    }
    
    public override async Task PublishedAsync(PublishContentContext context)
    {
        var payload = ContentItemEventPayload.Create(context.ContentItem);
        await webhookEventBroadcaster.BroadcastAsync(EventTypes.ContentItem.Published, payload);
    }

    public override async Task UnpublishedAsync(PublishContentContext context)
    {
        var payload = ContentItemEventPayload.Create(context.ContentItem);
        await webhookEventBroadcaster.BroadcastAsync(EventTypes.ContentItem.Unpublished, payload);
    }

    public override async Task RemovedAsync(RemoveContentContext context)
    {
        var payload = ContentItemEventPayload.Create(context.ContentItem);
        await webhookEventBroadcaster.BroadcastAsync(EventTypes.ContentItem.Removed, payload);
    }
}