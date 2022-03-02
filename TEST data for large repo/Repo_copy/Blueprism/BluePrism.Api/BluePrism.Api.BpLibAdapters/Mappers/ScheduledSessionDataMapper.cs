namespace BluePrism.Api.BpLibAdapters.Mappers
{
    using Domain;

    public static class ScheduledSessionDataMapper
    {
        public static ScheduledSession ToDomainModel(this Server.Domain.Models.ScheduledSession scheduledSession) =>
            new ScheduledSession
            {
                ProcessName = scheduledSession.ProcessName,
                ResourceName = scheduledSession.ResourceName,
            };
    }
}
