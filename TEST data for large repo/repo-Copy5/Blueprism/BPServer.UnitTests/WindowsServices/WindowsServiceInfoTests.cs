#if UNITTESTS

using System;
using NUnit.Framework;
using BluePrism.BPServer.WindowsServices;
using BluePrism.Core.WindowsSecurity;

namespace BluePrism.BPServer.UnitTests.WindowsServices
{
    /// <summary>
    /// Tests for the <see cref="WindowsServiceInfo"/> class.
    /// </summary>
    [TestFixture]
    public class WindowsServiceInfoTests
    {
        [Test]
        public void NullUserAccountShouldThrowArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => 
                new WindowsServiceInfo("C:\\BPServerServer.exe", "BP Server", null, "Manual", "Stopped", false ));
            
        }

        [Test]
        public void ConstructorShouldSetAllProperties()
        {   
            var servicePath = "C:\\BPServerServer.exe";
            var name = "BP Server";
            var acccount = new UserAccountIdentifier("LocalSystem", "S-1-5-18");
            var startMode = "Manual";
            var state = "Stopped";
            var hasPermissions = true;
                        
            var service = new WindowsServiceInfo(servicePath, name, acccount, startMode, state,hasPermissions);

            Assert.That(service.PathName, Is.EqualTo(servicePath));
            Assert.That(service.Name, Is.EqualTo(name));
            Assert.That(service.UserAccount, Is.EqualTo(acccount));
            Assert.That(service.StartMode, Is.EqualTo(startMode));
            Assert.That(service.State, Is.EqualTo(state));
            Assert.That(service.HasUrlPermission, Is.EqualTo(hasPermissions));

        }

    }
}

#endif