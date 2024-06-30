namespace OrchardExperiments.Webhooks.Payloads;

public record ContentItemPublished(
    string ContentType,
    string DisplayText,
    string Author,
    string Owner,
    string PublishedContentItemId,
    string? PreviousContentItemId);