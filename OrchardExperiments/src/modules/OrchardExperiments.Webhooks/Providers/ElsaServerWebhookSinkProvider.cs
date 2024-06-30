using WebhooksCore;

namespace OrchardExperiments.Webhooks.Providers;

public class ElsaServerWebhookSinkProvider : IWebhookSinkProvider
{
    public ValueTask<IEnumerable<WebhookSink>> ListWebhooksForEventAsync(string eventType, CancellationToken cancellationToken = default)
    {
        var endpoints = ListWebhooks().Where(x => x.EventTypes.Contains(eventType)).ToList();
        return new(endpoints);
    }
    
    private IEnumerable<WebhookSink> ListWebhooks()
    {
        yield return new WebhookSink
        {
            Id = "ElsaServer",
            Name = "Elsa Server",
            Url = new Uri("https://localhost:5001/elsa/api/webhooks"),
            EventTypes = [EventTypes.ContentItem.Published]
        };
    }
}