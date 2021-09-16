using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using BluePrism.AutomateAppCore;
using BluePrism.BPCoreLib;
using BluePrism.BPCoreLib.Collections;
using BluePrism.Core.Network;
using BluePrism.Scheduling;
using BluePrism.Scheduling.Calendar;
using NUnit.Framework;

namespace AutomateAppCore.UnitTests.DataContractTests
{
    [TestFixture]
    public class IServerDataContractTests
    {
        /// <summary>
        /// Types which dictate that they must be serialized using a NetDataContract
        /// serializer rather than the standard DataContract serializer.
        /// </summary>
        private static IList<Type> NetDataContractTypes = new[] { typeof(ScheduleList), typeof(SessionRunnerSchedule), typeof(ScheduledTask), typeof(PublicHolidaySchema), typeof(HistoricalScheduleLog), typeof(IScheduleLog) };

        /// <summary>
        /// Gets a Type representing the <see cref="IServer"/> interface
        /// </summary>
        /// <returns>The IServer Type instance</returns>
        private Type GetIServerType()
        {
            return GetPublicTypes("AutomateAppCore").FirstOrDefault(y => (y.FullName ?? "") == "BluePrism.AutomateAppCore.IServer");
        }

        /// <summary>
        /// WCF doesn't support generic methods on the service interface.
        /// Check for generic methods on IServer
        /// </summary>
        /// <remarks></remarks>
        [Test]
        public void TestIServerContainsNoGenericMethods()
        {
            var eyeServer = GetIServerType();
            Assert.NotNull(eyeServer);
            var genericMethods = new List<string>();
            var methods = eyeServer.GetMethods().ToList();
            foreach (var method in methods)
            {
                if (method.IsGenericMethod)
                {
                    genericMethods.Add(method.Name);
                }
            }

            if (genericMethods.Any())
            {
                Assert.Fail("IServer must not contain generic methods. Generic methods found: " + Environment.NewLine + string.Join(Environment.NewLine, genericMethods));
            }
        }

        /// <summary>
        /// Tests that the data classes used by the IServer interface have [DataContract] and [Seralizable] attributes.
        /// </summary>
        /// <remarks></remarks>
        [Test]
        public void TestIServerTypesHaveAttributes()
        {

            // A list of errors found when checking classes, to be displayed on test failure
            var classCheckErrors = new List<string>();

            // Check types for required attributes
            foreach (var type in GetTypesToCheck())
            {
                Trace.WriteLine(type.FullName);
                var result = CheckAttributesOnClass(type);
                if (!string.IsNullOrEmpty(result))
                    classCheckErrors.Add(result);
            }

            if (classCheckErrors.Count > 0)
            {
                Assert.Fail("Errors were found in {0} types exposed by WCF: {1}{2}", classCheckErrors.Count, Environment.NewLine, string.Join(Environment.NewLine, classCheckErrors));
            }
        }

        /// <summary>
        /// Tests that the [DataContract] attributes have namespaces.
        /// </summary>
        /// <remarks></remarks>
        [Test]
        public void TestDataContractAttributesHaveNamespaces()
        {
            var classCheckErrors = new List<string>();

            // Check types for required attributes
            foreach (var type in GetTypesToCheck())
            {
                Trace.WriteLine(type.FullName);
                var result = CheckNamespacePropertyOnAttributes(type);
                if (!string.IsNullOrEmpty(result))
                    classCheckErrors.Add(result);
            }

            if (classCheckErrors.Count > 0)
            {
                Assert.Fail("Errors were found in {0} types exposed by WCF: {1}{2}", classCheckErrors.Count, Environment.NewLine, string.Join(Environment.NewLine, classCheckErrors));
            }
        }


        /// <summary>
        /// Populates collection with types to be tested
        /// </summary>
        private IEnumerable<Type> GetTypesToCheck()
        {
            var typesToCheck = GetIServerDataClassTypes();
            var count = 0;
            while (count < typesToCheck.Count)
            {
                var t = typesToCheck[count];
                Trace.WriteLine("Discovered class: " + t.FullName);
                DiscoverTypesInClass(t, typesToCheck);
                count += 1;
            }

            Trace.WriteLine("Discovered " + count + " classes to check.");
            return typesToCheck;
        }

        /// <summary>
        /// Adds the type t to the discovered types list if it passes various checks.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="discoveredTypes"></param>
        /// <remarks></remarks>
        private void AddTypeToDiscoveredTypesList(Type t, List<Type> discoveredTypes)
        {
            if (t is null)
                return;
            if (discoveredTypes.Contains(t))
                return;
            if (IsTypeInExceptionList(t))
                return;
            if (t.IsEnum)
                return;
            if (IsBuiltInType(t))
                return;
            discoveredTypes.Add(t);
        }

        /// <summary>
        /// Compares the type to a list of types which don't need checking for attributes.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        /// <remarks>Types in the exception list are excluded from the test
        /// - these have been checked and confirmed that the types don't need the
        /// DataContract and Serializable attributes</remarks>
        private bool IsTypeInExceptionList(Type t)
        {
            var exceptions = new List<string>() { "SafeString", "clsProgressMonitor", "AccessConstraintMap`2", "SystemRoleSet", "TrackingRoleSet", "BackgroundWorkerProgressMonitor", "clsProcess", "BasicDataMonitor", "CharData", "DummySchedule", "FilteringGroup", "DateBasedCalendar", "TesterSchedule", "FakeSessionLog" };
            return exceptions.Contains(t.Name);
        }

        /// <summary>
        /// Returns a list of all types used directly on the IServer interface.
        /// </summary>
        /// <returns></returns>
        /// <remarks></remarks>
        private List<Type> GetIServerDataClassTypes()
        {
            var eyeServer = GetIServerType();
            Assert.NotNull(eyeServer);
            var discoveredTypes = new List<Type>();
            var methods = eyeServer.GetMethods().ToList();
            foreach (MethodInfo m in methods)
            {
                Type type;
                if (m.IsPublic & !m.IsConstructor & !m.IsGenericMethod)
                {
                    if (m.GetParameters() is object)
                    {
                        foreach (ParameterInfo p in m.GetParameters())
                        {
                            if (p.ParameterType is object)
                            {
                                type = p.ParameterType;
                                if (!p.ParameterType.IsByRef)
                                {
                                    if (type.IsArray)
                                        type = type.GetElementType();
                                    AddTypeToDiscoveredTypesList(type, discoveredTypes);
                                }
                            }
                        }
                    }

                    if (m.ReturnType is object)
                    {
                        type = m.ReturnType;
                        if (type.IsArray)
                            type = type.GetElementType();
                        AddTypeToDiscoveredTypesList(type, discoveredTypes);
                    }
                }
            }

            return discoveredTypes;
        }


        /// <summary>
        /// Returns true if the type t is a .NET type.
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        /// <remarks></remarks>
        private bool IsBuiltInType(Type t)
        {
            return (t.Module.ScopeName ?? "") == "CommonLanguageRuntimeLibrary" || t.Namespace is object && t.Namespace.StartsWith("System");
        }


        /// <summary>
        /// Given type t, populates the list discoveredTypes with types used in the public properties / field of t.
        /// Ignores any types which already exist in list discoveredTypes.
        /// The base class of t will also be added to the discoveredTypes list if not already.
        /// Built in .NET types are ignored.
        /// </summary>
        /// <param name="t"></param>
        /// <param name="discoveredTypes"></param>
        /// <remarks></remarks>
        private void DiscoverTypesInClass(Type t, List<Type> discoveredTypes)
        {
            if (t.IsArray)
            {
                t = t.GetElementType();
                AddTypeToDiscoveredTypesList(t, discoveredTypes);
            }

            // Check base type, if not already in the list.
            AddTypeToDiscoveredTypesList(t.BaseType, discoveredTypes);


            // Check derived types
            var derivedTypes = GetDerivedTypes(t);
            foreach (var dt in derivedTypes)
                AddTypeToDiscoveredTypesList(dt, discoveredTypes);

            // Properties and public fields marked with a DataMember attribute also need to 
            // be checked to ensure they have the correct attributes.
            // Add them to the foundTypes list so they will be checked later.
            foreach (var member in t.GetMembers().Where(x => x.GetCustomAttribute<DataMemberAttribute>(true) is object))
            {

                // Check the member types
                Type type = null;

                // We only need to bother with properties and fields
                var switchExpr = member.MemberType;
                switch (switchExpr)
                {
                    case MemberTypes.Property:
                        type = ((PropertyInfo)member).PropertyType;
                        break;
                    
                    case MemberTypes.Field:
                        type = ((FieldInfo)member).FieldType;
                        break;
                }

                if (type is null)
                {
                    continue;
                }

                // If it is an array, get the element type.
                if (type.IsArray)
                {
                    type = type.GetElementType();
                }

                AddTypeToDiscoveredTypesList(type, discoveredTypes);
            }
        }

        /// <summary>
        /// Returns a list of all the types derived from the type passed as argument
        /// </summary>
        /// <param name="t">The types for which all derived types are required</param>
        /// <returns>An enumerable over the derived types of <paramref name="t"/> in
        /// all currently loaded assemblies</returns>
        private IEnumerable<Type> GetDerivedTypes(Type t)
        {
            return AppDomain.CurrentDomain.GetAssemblies().SelectMany(a =>
            {
                Type[] types;
                try
                {
                    types = a.GetTypes();
                }
                catch (ReflectionTypeLoadException ex)
                {
                    types = ex.Types;
                }

                if (t.IsInterface)
                {
                    return types.Where(ty => t.IsAssignableFrom(ty) && ty.IsClass && !ty.FullName.StartsWith("Castle.Proxies."));
                }
                else
                {
                    return types.Where(ty => ty is object && ty.IsSubclassOf(t));
                }
            });
        }


        /// <summary>
        /// Check the type has the [DataContract] and [Serializable] attributes.
        /// </summary>
        /// <param name="t">The type of data being used in IServer</param>
        /// <returns>An error message indicating any problems, or an empty string if no
        /// errors were encountered</returns>
        private string CheckAttributesOnClass(Type t)
        {
            // WCF classes should be -
            // serializable
            // datacontract
            // all members should be marked as datamember - or not? and serializable themnselves
            if (t.IsNestedPrivate)
            {
                Trace.WriteLine("Bypassing private nested class - " + t.FullName);
                return string.Empty;
            }

            if (t.IsInterface)
            {
                Trace.WriteLine("Bypassing interface - " + t.FullName);
                return string.Empty;
            }

            // Check that the class has the required attributes
            var missing = new List<string>(2);
            if (t.GetCustomAttribute<DataContractAttribute>(true) is null)
                missing.Add("DataContract");
            if (t.GetCustomAttribute<SerializableAttribute>(true) is null)
                missing.Add("Serializable");
            if (missing.Count > 0)
                return t.FullName + " missing " + string.Join(", ", missing);
            return "";
        }

        private string CheckNamespacePropertyOnAttributes(Type type)
        {
            if (type.IsNestedPrivate)
            {
                Trace.WriteLine("Bypassing private nested class - " + type.FullName);
                return string.Empty;
            }

            if (type.IsInterface)
            {
                Trace.WriteLine("Bypassing interface - " + type.FullName);
                return string.Empty;
            }

            if (type.GetCustomAttribute<DataContractAttribute>(true).IsNamespaceSetExplicitly)
            {
                return string.Empty;
            }

            return $"The type {type.Name} data contract attribute does not have a namespace property.";
        }


        /// <summary>
        /// Returns a list of public types in an assembly
        /// </summary>
        /// <param name="assemblyName"></param>
        /// <returns></returns>
        private IEnumerable<Type> GetPublicTypes(string assemblyName)
        {
            return Assembly.Load(assemblyName).GetTypes().Where(y => y.IsPublic);
        }

        [Test(Description = "Tests that any data type which requires a NetDataContractSerializable is used " + "only in operations with the appropriate attribute assigned")]
        public void TestNetDataContractSerializables()
        {
            var eyeServer = GetIServerType();
            Assert.NotNull(eyeServer);
            foreach (var m in eyeServer.GetMethods())
            {
                foreach (var tp in m.GetReferencedTypes())
                {
                    if (!NetDataContractTypes.Contains(tp))
                        continue;
                    Assert.That(m.GetCustomAttribute<UseNetDataContractSerializerAttribute>(), Is.Not.Null, "IServer Method '{0}' uses type '{1}', which requires the UseNetDataContractSerializer attribute", m.Name, tp.FullName);
                }
            }
        }

        [Test(Description = "Tests that any [Flags] enums have the 'EnumMember' attribute applied to their " + "member values")]
        public void TestFlagsEnumValuesHaveMarkup()
        {
            var eyeServer = GetIServerType();
            Assert.NotNull(eyeServer);
            var processedEnums = new clsGeneratorDictionary<Type, List<string>>();
            var failedEnums = new clsGeneratorDictionary<Type, List<string>>();

            // Go through all types referenced (directly) in IServer;
            // For any Enums with the [Flags] attribute set, ensure that all of its
            // members have the [EnumMember] attribute set.
            foreach (var tp in eyeServer.GetAllReferencedTypes())
            {
                if (IsBuiltInType(tp))
                    continue;
                if (!processedEnums.ContainsKey(tp))
                    processedEnums.Add(tp, new List<string>());
                processedEnums[tp].AddRange(CheckTypeForWCF(tp));

                // Lets see if any of this objects members are candidates.
                tp.GetAllReferencedTypes().ToList().ForEach(n =>
                {
                    if (IsBuiltInType(n))
                        return;
                    if (!processedEnums.ContainsKey(n))
                        processedEnums.Add(n, new List<string>());
                    processedEnums[n].AddRange(CheckTypeForWCF(n));
                });
            }

            // Strip out the ones with fails
            foreach (Type x in processedEnums.Keys)
            {
                if (processedEnums[x] is object && processedEnums[x].Count > 0)
                {
                    failedEnums.Add(x, processedEnums[x]);
                }
            }

            Assert.That(failedEnums, Is.Empty, "[Flags] Enum values found with no [EnumMember] Attribute: {0}{1}", Environment.NewLine, string.Join(Environment.NewLine, failedEnums.Select(p => p.Key.FullName + ": " + string.Join(", ", p.Value))));
        }

        /// <summary>
        /// Checks an individual type for compatibility with WCF
        /// </summary>
        /// <param name="t"></param>
        /// <returns></returns>
        private List<string> CheckTypeForWCF(Type t)
        {
            var fails = new List<string>();
            if (!t.IsEnum)
                return fails;
            if (t.GetCustomAttribute<FlagsAttribute>() is null)
                return fails;
            if (t.GetCustomAttribute<DataContractAttribute>() is null)
                fails.Add("<Needs DataContract Attribute>");
            foreach (Enum value in Enum.GetValues(t))
            {
                if (BPUtil.GetAttributeValue(typeof(EnumMemberAttribute), value) is null)
                    fails.Add(value.ToString());
            }

            return fails;
        }
    }
}
