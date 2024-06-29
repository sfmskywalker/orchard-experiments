using WebhooksCore;

namespace OrchardExperiments.Webhooks.Providers;

public class ElsaServerWebhookEndpointsSource : IWebhookEndpointsSource
{
    public ValueTask<IEnumerable<WebhookEndpoint>> ListWebhooksForEventAsync(string eventType, CancellationToken cancellationToken = default)
    {
        var endpoints = ListWebhooks().Where(x => x.EventTypes.Contains(eventType)).ToList();
        return new(endpoints);
    }
    
    private IEnumerable<WebhookEndpoint> ListWebhooks()
    {
        yield return new WebhookEndpoint
        {
            Id = "1",
            Name = "Elsa Server",
            Url = new Uri("https://localhost:5001/orchard/webhooks"),
            EventTypes = new HashSet<string>
            {
                EventTypes.ContentItem.Published
            }
        };
    }
}