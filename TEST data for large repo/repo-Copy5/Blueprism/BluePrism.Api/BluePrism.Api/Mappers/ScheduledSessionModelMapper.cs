namespace BluePrism.Api.Mappers
{
    using Domain;
    using Models;

    public static class ScheduledSessionModelMapper
    {
        public static ScheduledSessionModel ToModel(this ScheduledSession session) => new ScheduledSessionModel
        {
            ProcessName = session.ProcessName,
            ResourceName = session.ResourceName
        };
    }
}
