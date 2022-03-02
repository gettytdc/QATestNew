namespace BluePrism.Api.Mappers
{
    using Domain;
    using Models;

    public static class ScheduledSessionMapper
    {
        public static ScheduledSessionModel ToModelObject(this ScheduledSession session) => new ScheduledSessionModel
        {
            ProcessName = session.ProcessName,
            ResourceName = session.ResourceName
        };
    }
}
