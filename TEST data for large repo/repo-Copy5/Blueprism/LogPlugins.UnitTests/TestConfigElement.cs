#if UNITTESTS
namespace LogPlugins.UnitTests
{
    using BluePrism.Core.Plugins;

    public class TestConfigElement : IConfigElement
    {
        public string Name { get; }
        public object Value { get; set; }

        public TestConfigElement(string name, object value)
        {
            Name = name;
            Value = value;
        }
    }
}
#endif