namespace OrchardExperiments.Webhooks.Payloads;

public record ContentItemPublished(string PublishedContentItemId, string? PreviousContentItemId);