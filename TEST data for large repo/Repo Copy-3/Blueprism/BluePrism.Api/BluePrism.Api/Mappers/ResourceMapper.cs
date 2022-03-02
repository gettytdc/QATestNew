namespace BluePrism.Api.Mappers
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using Domain;
    using Models;

    using ResourceAttribute = Domain.ResourceAttribute;
    using ResourceDbStatus = Domain.ResourceDbStatus;
    using ResourceDisplayStatus = Domain.ResourceDisplayStatus;

    public static class ResourceMapper
    {
        public static ResourceModel ToModelObject(this Resource @this) =>
            new ResourceModel
            {
                Id = @this.Id,
                PoolId = @this.PoolId,
                Name = @this.Name,
                PoolName = @this.PoolName,
                GroupId = @this.GroupId,
                GroupName = @this.GroupName,
                ActiveSessionCount = @this.ActiveSessionCount,
                WarningSessionCount = @this.WarningSessionCount,
                PendingSessionCount = @this.PendingSessionCount,
                Attributes = @this.Attributes.ToModel(),
                DatabaseStatus = @this.DatabaseStatus.ToModel(),
                DisplayStatus = @this.DisplayStatus.ToModel()
            };

        public static IEnumerable<ResourceModel> ToModel(this IEnumerable<Resource> @this) =>
            @this.Select(x => x.ToModelObject());

        public static Resource ToDomain(this UpdateResourceModel @this) =>
            new Resource { Attributes = @this.Attributes.ToDomain() };

        private static IEnumerable<Models.ResourceAttribute> ToModel(this ResourceAttribute resourceAttribute) =>
            resourceAttribute.ToString()
                .Split(new[] { ", " }, StringSplitOptions.None)
                .Select(MapResourceAttribute);

        private static ResourceAttribute ToDomain(this IEnumerable<Models.ResourceAttribute> @this)
        {
            var aggregatedEnum = Enum.GetValues(typeof(Models.ResourceAttribute))
                .Cast<Models.ResourceAttribute>()
                .Where(@this.Contains)
                .Select(MapToResourceAttributeDomain)
                .Aggregate(0, (current, value) => current | Convert.ToInt32(value));

            return (ResourceAttribute)Enum.Parse(typeof(ResourceAttribute), aggregatedEnum.ToString());
        }

        private static Models.ResourceDbStatus ToModel(this ResourceDbStatus resourceDbStatus)
        {
            switch (resourceDbStatus)
            {
                case ResourceDbStatus.Unknown:
                    return Models.ResourceDbStatus.Unknown;
                case ResourceDbStatus.Ready:
                    return Models.ResourceDbStatus.Ready;
                case ResourceDbStatus.Offline:
                    return Models.ResourceDbStatus.Offline;
                case ResourceDbStatus.Pending:
                    return Models.ResourceDbStatus.Pending;
                default:
                    throw new ArgumentException("Unexpected resource db status", nameof(resourceDbStatus));
            }
        }

        private static Models.ResourceDisplayStatus ToModel(this ResourceDisplayStatus resourceDisplayStatus)
        {
            switch (resourceDisplayStatus)
            {
                case ResourceDisplayStatus.Idle:
                    return Models.ResourceDisplayStatus.Idle;
                case ResourceDisplayStatus.LoggedOut:
                    return Models.ResourceDisplayStatus.LoggedOut;
                case ResourceDisplayStatus.Missing:
                    return Models.ResourceDisplayStatus.Missing;
                case ResourceDisplayStatus.Private:
                    return Models.ResourceDisplayStatus.Private;
                case ResourceDisplayStatus.Warning:
                    return Models.ResourceDisplayStatus.Warning;
                case ResourceDisplayStatus.Working:
                    return Models.ResourceDisplayStatus.Working;
                case ResourceDisplayStatus.Offline:
                    return Models.ResourceDisplayStatus.Offline;
                default:
                    throw new ArgumentException("Unexpected resource display status", nameof(resourceDisplayStatus));
            }
        }

        private static Models.ResourceAttribute MapResourceAttribute(string attribute)
        {
            if (Enum.TryParse<ResourceAttribute>(attribute, out var result))
                return MapToResourceAttributeModel(result);

            throw new ArgumentException("Unable to parse resource attribute", nameof(attribute));
        }

        private static Models.ResourceAttribute MapToResourceAttributeModel(ResourceAttribute resourceAttribute)
        {
            switch (resourceAttribute)
            {
                case ResourceAttribute.None:
                    return Models.ResourceAttribute.None;
                case ResourceAttribute.Local:
                    return Models.ResourceAttribute.Local;
                case ResourceAttribute.LoginAgent:
                    return Models.ResourceAttribute.LoginAgent;
                case ResourceAttribute.Retired:
                    return Models.ResourceAttribute.Retired;
                case ResourceAttribute.Private:
                    return Models.ResourceAttribute.Private;
                case ResourceAttribute.DefaultInstance:
                    return Models.ResourceAttribute.DefaultInstance;
                default:
                    throw new ArgumentException("Unexpected resource attribute", nameof(resourceAttribute));
            }
        }

        private static ResourceAttribute MapToResourceAttributeDomain(Models.ResourceAttribute resourceAttribute)
        {
            switch (resourceAttribute)
            {
                case Models.ResourceAttribute.None:
                    return ResourceAttribute.None;
                case Models.ResourceAttribute.Local:
                    return ResourceAttribute.Local;
                case Models.ResourceAttribute.LoginAgent:
                    return ResourceAttribute.LoginAgent;
                case Models.ResourceAttribute.Retired:
                    return ResourceAttribute.Retired;
                case Models.ResourceAttribute.Private:
                    return ResourceAttribute.Private;
                case Models.ResourceAttribute.DefaultInstance:
                    return ResourceAttribute.DefaultInstance;
                default:
                    throw new ArgumentException("Unexpected resource attribute", nameof(resourceAttribute));
            }
        }
    }
}
