using OrchardExperiments.Webhooks.Payloads;
using WebhooksCore;

namespace OrchardExperiments.Webhooks.Providers;

public class ElsaServerWebhookSinkProvider : IWebhookSinkProvider
{
    public ValueTask<IEnumerable<WebhookSink>> ListAsync(CancellationToken cancellationToken = default)
    {
        var endpoints = ListWebhooks().ToList();
        return new(endpoints);
    }
    
    private IEnumerable<WebhookSink> ListWebhooks()
    {
        yield return new WebhookSink
        {
            Id = "ElsaServer",
            Name = "Elsa Server",
            Url = new Uri("https://localhost:5001/elsa/api/webhooks"),
            Filters = [new WebhookEventFilter
            {
                EventType = EventTypes.ContentItem.Published,
                PayloadFilters = 
                {
                    new PayloadFilter(nameof(ContentItemPublished.ContentType), "Article"),
                    new PayloadFilter(nameof(ContentItemPublished.ContentType), "BlogPost")
                }
            }]
        };
    }
}