namespace BluePrism.Api.BpLibAdapters.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using AutomateAppCore;
    using AutomateProcessCore;
    using Domain;
    using Func;

    public static class CreateWorkQueueItemsMapper
    {
        public static IEnumerable<CreateWorkQueueItemRequest> ToBluePrismObject(this IEnumerable<CreateWorkQueueItem> @this) =>
            @this.Select(x => x.ToBluePrismObject());

        public static CreateWorkQueueItemRequest ToBluePrismObject(this CreateWorkQueueItem @this) =>
            new CreateWorkQueueItemRequest
            {
                Status = @this.Status,
                Priority = @this.Priority,
                Defer = @this.DeferredDate.ToBluePrismObject(),
                Tags = @this.Tags.ToBluePrismTagExpectedFormat(),
                Data = @this.Data == null ? new clsCollection() : @this.Data.ToBluePrismObject()
            };

        private static string ToBluePrismTagExpectedFormat(this IReadOnlyCollection<string> @this) =>
            @this?.Map(string.Join, ";") ?? string.Empty;

        private static DateTime ToBluePrismObject(this DateTimeOffset? @this) =>
            @this?.DateTime ?? DateTime.MinValue;
    }
}
