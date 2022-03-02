#if UNITTESTS
namespace LogPlugins.UnitTests
{
    using System.Linq;
    using System.Reflection;

    using BluePrism.BPCoreLib;
    using BluePrism.Core.Plugins;

    using NUnit.Framework;

    [TestFixture]
    public class RequestFactoryTests
    {
        [Test]
        public void EnsureIRequestFactoryClassesHaveRequiredConstructor()
        {
            var result =
                Assembly.GetExecutingAssembly()
                    .GetConcreteImplementations<IRequestFactory>()
                    .Select(x => x.GetConstructor(new[] { typeof(IConfiguration) }))
                    .All(x => x != null);

            Assert.IsTrue(result);
        }

        [Test]
        public void EnsureIRequestFactoryClassesHaveRequiredStaticMethod()
        {
            var result = 
                Assembly.GetExecutingAssembly()
                    .GetConcreteImplementations<IRequestFactory>()
                    .Select(x => x.GetMethod("ConfigurationIsSuitable", BindingFlags.Static | BindingFlags.Public))
                    .All(x => x != null);

            Assert.IsTrue(result);
        }
    }
}
#endif