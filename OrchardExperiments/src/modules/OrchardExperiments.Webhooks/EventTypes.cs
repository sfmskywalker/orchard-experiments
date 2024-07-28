namespace OrchardExperiments.Webhooks;

public static class EventTypes
{
    public static class ContentItem
    {
        public const string Created = "content-item.created";
        public const string Published = "content-item.published";
        public const string Unpublished = "content-item.unpublished";
        public const string Removed = "content-item.removed";
    }
}