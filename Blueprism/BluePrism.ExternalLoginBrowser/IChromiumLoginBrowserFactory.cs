namespace BluePrism.ExternalLoginBrowser
{
    public interface IChromiumLoginBrowserFactory
    {
        IChromiumLoginBrowser Create(string startUrl, string endUrl);
    }
}
