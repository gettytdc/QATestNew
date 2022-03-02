namespace BluePrism.StartUp.Modules
{
    using System.Security.Cryptography;
    using ApplicationManager.BrowserAutomation;
    using Autofac;
    using BrowserAutomation;
    using BrowserAutomation.Cryptography;
    using WebSocketSharp.Server;
    using BluePrism.BrowserAutomation.NativeMessaging;
    using BluePrism.BrowserAutomation.NamedPipe;

    public class BrowserAutomationModule : Module
    {
        protected override void Load(ContainerBuilder builder)
        {
            builder.RegisterType<NativeMessagingWebPageProvider>().As<IWebPageProvider>().SingleInstance();

            builder.RegisterType<WebSocketServer>().AsSelf();

            builder.RegisterType<NamedPipeWrapper>().As<INamedPipeWrapper>().SingleInstance();

            builder.RegisterType<MessageCryptographyProvider>().As<IMessageCryptographyProvider>();
            builder.RegisterType<NativeMessagingWebPage>().As<IWebPage>();
            builder.RegisterType<WebElement>().As<IWebElement>();
            builder.RegisterType<HashAlgorithmWrapper<SHA256CryptoServiceProvider>>().As<IHashAlgorithm>().ExternallyOwned();
            builder.RegisterType<SymmetricAlgorithmWrapper<AesCryptoServiceProvider>>().As<ISymmetricAlgorithm>().ExternallyOwned();
            builder.RegisterType<CryptoStreamWrapper>().As<ICryptoStream>().ExternallyOwned();
            builder.RegisterType<SHA256CryptoServiceProvider>().AsSelf().ExternallyOwned();
            builder.RegisterType<AesCryptoServiceProvider>().AsSelf().ExternallyOwned();

            builder.RegisterType<BrowserAutomationIdentifierHelper>().As<IBrowserAutomationIdentifierHelper>();
        }
    }
}
