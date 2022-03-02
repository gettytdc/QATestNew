using System.Threading.Tasks;

namespace BluePrism.ExternalLoginBrowser.Cookies
{
    public interface ICookieManager
    {
        Task DeleteIdentityCookieFromBrowser();
    }
}
