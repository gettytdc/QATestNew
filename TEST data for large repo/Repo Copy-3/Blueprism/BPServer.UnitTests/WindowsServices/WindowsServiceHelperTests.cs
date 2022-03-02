#if UNITTESTS

using NUnit.Framework;
using BluePrism.BPServer.WindowsServices;

namespace BluePrism.BPServer.UnitTests.WindowsServices
{
    /// <summary>
    /// Tests for the <see cref="WindowsServiceHelper"/> class.
    /// </summary>
    [TestFixture]
    public class WindowsServiceHelperTests
    {
        [Test]
        [TestCase("C:\\Program Files\\Blue Prism Limited\\Blue Prism Automate\\BPServerService.exe", "Default", ExpectedResult = true)]
        [TestCase("\"C:\\Program Files\\Blue Prism Limited\\Blue Prism Automate\\BPServerService.exe\"", "Default", ExpectedResult = true)]
        [TestCase("C:\\Program Files (x86)\\Blue Prism Limited\\Blue Prism Automate\\BPServerService.exe Default", "Default", ExpectedResult = true)]
        [TestCase("C:\\My Services\\BPServerService.exe Profile1", "Profile1", ExpectedResult = true)]
        [TestCase("\"C:\\My Services\\BPServerService.exe\" Profile1", "Profile1", ExpectedResult = true)]
        [TestCase("C:\\My Services\\BPServerService.exe Profile1", "profile1", ExpectedResult = true)]
        [TestCase("C:\\My Services\\BPServerService.exe \"Profile 1\"", "Profile 1", ExpectedResult = true)]
        [TestCase("\"C:\\My Services\\BPServerService.exe\" \"Profile 1\"", "Profile 1", ExpectedResult = true)]
        [TestCase("C:\\Program Files (x86)\\Blue Prism Limited\\Blue Prism Automate\\BPServerService.exe", "Profile1", ExpectedResult = false)]
        [TestCase("C:\\My Services\\BPServerService.exe Profile1", "Profile2", ExpectedResult = false)]
        [TestCase("C:\\My Services\\BPServerService.exe Profile1 Profile2", "Profile2", ExpectedResult = false)]
        [TestCase("C:\\My Services\\BPServerService.exe \"Profile1\" \"Profile2\"", "Profile1", ExpectedResult = false)]
        [TestCase("C:\\Program Files\\Blue Prism Limited\\Blue Prism Automate\\AlternateService.exe Profile1", "Profile1", ExpectedResult = false)]
        public bool TestIsServerAssociatedWithProfile(string servicePath, string configName)
        {
            var helper = new WindowsServiceHelper();
            return helper.IsServerAssociatedWithConfiguration(servicePath, configName);
        }

        [TestCase( ".\\bfalconer", ExpectedResult = "bfalconer")]
        [TestCase("MyDomain\bfalconer", ExpectedResult = "MyDomain\bfalconer")]
        public string ShouldRemoveBuiltInDomainFromAccountName(string accountName)
        {
            var helper = new WindowsServiceHelper();
            return helper.RemoveBuiltInDomainFromAccountName(accountName);
        }
        
        [TestCase("Default", ExpectedResult = @"sc create ""Blue Prism Server:Default"" binPath= ""C:\My Services\BPServerService.exe \""Default\""""")]
        [TestCase("Server 1", ExpectedResult = @"sc create ""Blue Prism Server:Server 1"" binPath= ""C:\My Services\BPServerService.exe \""Server 1\""""")]
        public string TestGetCreateWindowsServiceCommand(string configName)
        {
            var directory = @"C:\My Services";
            var helper = new WindowsServiceHelper(directory);
            return helper.GetCreateWindowsServiceCommandText(configName);
        }

        [TestCase("Default", ExpectedResult = @"C:\My Services\BPServerService.exe ""Default""") ]
        [TestCase("Server 1", ExpectedResult = @"C:\My Services\BPServerService.exe ""Server 1""")]
        public string TestGetServicePathForCurrentDirectory(string configName)
        {
            var directory = @"C:\My Services";
            var helper = new WindowsServiceHelper(directory);
            return helper.GetServicePathForCurrentDirectory(configName);
        }
    }
    
    
}

#endif