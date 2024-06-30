using OrchardCore.ContentManagement.Handlers;
using OrchardExperiments.Webhooks.Payloads;
using WebhooksCore;

namespace OrchardExperiments.Webhooks.Handlers;

public class ContentEventHandlers(IWebhookEventBroadcaster webhookEventBroadcaster) : ContentHandlerBase
{
    public override async Task PublishedAsync(PublishContentContext context)
    {
        var publishedContentItem = context.PublishingItem;
        var previousContentItem = context.PreviousItem;
        
        var payload = new ContentItemPublished(
            publishedContentItem.ContentType,
            publishedContentItem.DisplayText,
            publishedContentItem.Author,
            publishedContentItem.Owner,
            publishedContentItem.ContentItemId,
            previousContentItem?.ContentItemId);
        
        await webhookEventBroadcaster.BroadcastAsync(EventTypes.ContentItem.Published, payload);
    }
}