namespace BluePrism.ExternalLoginBrowser
{
    public interface IBrowserFormFactory
    {
        IBrowserForm Create(IChromiumLoginBrowser browser);
    }
}