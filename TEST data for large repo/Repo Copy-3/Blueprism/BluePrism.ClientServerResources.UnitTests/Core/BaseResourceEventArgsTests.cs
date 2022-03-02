using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using BluePrism.ClientServerResources.Core.Events;
using NUnit.Framework;

namespace BluePrism.ClientServerResources.UnitTests.Core
{
    [TestFixture]
    public class BaseResourceEventArgsTests
    {
        private IEnumerable<Type> GetSubclasses<T>() where T : class
        {
            foreach (var type in
                Assembly.GetAssembly(typeof(T)).GetTypes()
                .Where(myType => myType.IsClass && !myType.IsAbstract && myType.IsSubclassOf(typeof(T))))
            {
                yield return type;
            }
        }

        [Test]
        public void SessionEventArgs_EnsureImplementersOverrideFromScheduler()
        {
            var bindingFlags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.DeclaredOnly;
            var propname = nameof(BaseResourceEventArgs.FromScheduler);
            var abstractInfo = typeof(BaseResourceEventArgs).GetProperty(propname);

            foreach(var type in GetSubclasses<BaseResourceEventArgs>())
            {
                // check that the implemented class contains a property for scheduledsessionid
                var hasProperty = type.GetProperties(bindingFlags).Any(x => x.Name.ToLower().Contains("sched") && x.PropertyType == typeof(int));

                if (hasProperty)
                {
                    var propDetails = type.GetProperty(propname, bindingFlags);

                    Assert.IsNotNull(propDetails, $"{propname} must be overridden for type {type.Name}");
                    Assert.AreNotEqual(abstractInfo.GetGetMethod(), propDetails.GetGetMethod());
                }
            }
        }



    }
}
