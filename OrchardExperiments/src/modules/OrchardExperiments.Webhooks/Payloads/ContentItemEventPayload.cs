using OrchardCore.ContentManagement;

namespace OrchardExperiments.Webhooks.Payloads;

public record ContentItemEventPayload(
    string ContentType,
    string DisplayText,
    string Author,
    string Owner,
    string ContentItemId)
{
    public static ContentItemEventPayload Create(ContentItem contentItem)
    {
        var contentType = contentItem.ContentType;
        var displayText = contentItem.DisplayText;
        var author = contentItem.Author;
        var owner = contentItem.Owner;
        var contentItemId = contentItem.ContentItemId;

        return new ContentItemEventPayload(contentType, displayText, author, owner, contentItemId);
    }
}