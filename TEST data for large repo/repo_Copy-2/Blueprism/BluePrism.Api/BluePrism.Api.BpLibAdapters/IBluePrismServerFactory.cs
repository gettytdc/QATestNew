namespace BluePrism.Api.BpLibAdapters
{
    using AutomateAppCore;

    public interface IBluePrismServerFactory
    {
        IServer ClientInit();
    }
}
