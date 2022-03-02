namespace BluePrism.Logging
{
    public interface ILoggerFactory
    {
        NLog.ILogger GetLogger(string name);
    }
}
