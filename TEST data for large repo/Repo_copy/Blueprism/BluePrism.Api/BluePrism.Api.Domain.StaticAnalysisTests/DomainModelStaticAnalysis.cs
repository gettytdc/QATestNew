namespace BluePrism.Api.Domain.StaticAnalysisTests
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Reflection;
    using NUnit.Framework;

    [TestFixture]
    public class DomainModelStaticAnalysis
    {
        private static readonly IReadOnlyCollection<(Type Type, string Method)> Exceptions = new (Type, string)[] { };

        private static readonly string[] StandardMethods =
        {
            "Finalize",
            "MemberwiseClone",
            "Equals",
            "ToString",
            "GetHashCode",
            "GetType",
            "CompareTo",
            "HasFlag",
            "GetTypeCode",
            "GetBaseException",
            "GetObjectData",
            "op_Implicit",
            "GetEnumerator",
            "GetValue",
            "GetHashCodeForValidation",
        };

        private static readonly Type[] IgnoredDeclaringTypes =
        {
            typeof(Exception),
        };
#if DEBUG
        [Test]
        public void DomainModels_ShouldNotExposeMethods()
        {
            var unexpectedMethods =
                typeof(WorkQueue).Assembly // Arbitrary type to get domain assembly
                .GetExportedTypes()
                .Select(x => (Type: x, Methods: GetAllRelevantMethods(x)))
                .SelectMany(x => x.Methods.Select(m => (Type: x.Type, Method: m.Name)))
                .Where(x => !StandardMethods.Contains(x.Method))
                .Where(x => !x.Method.StartsWith("get_") && !x.Method.StartsWith("set_"))
                .Except(Exceptions)
                .ToArray();

            if(unexpectedMethods.Any())
                Assert.Fail($"The following methods were found on domain models. In general, models should not contain methods; if interaction is required, extension methods are recommended. If this is intentional and needed then add an exception in the class that contains this test.\r\n\r\n{string.Join("\r\n", unexpectedMethods.Select(x => $"{x.Type.FullName}.{x.Method}"))}");
        }
#endif
        private IEnumerable<MethodInfo> GetAllRelevantMethods(Type type) =>
            type.GetMethods()
                .Concat(
                    type.GetMethods(BindingFlags.NonPublic | BindingFlags.Instance)
                        .Where(m => !IgnoredDeclaringTypes.Contains(m.DeclaringType))
                        .Where(m => !m.IsVirtual));
    }
}
