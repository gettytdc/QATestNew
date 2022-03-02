namespace BluePrism.Api.CommonTestClasses.Extensions
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using NUnit.Framework;

    public static class TestCaseExtensions
    {
        public static IEnumerable<TestCaseData> ToTestCaseData<T1, T2, T3, T4>(this IEnumerable<ValueTuple<T1, T2, T3, T4>> values) =>
            values.Select(x => new TestCaseData(x.Item1, x.Item2, x.Item3, x.Item4));

        public static IEnumerable<TestCaseData> ToTestCaseData<T1, T2, T3>(this IEnumerable<ValueTuple<T1, T2, T3>> values) =>
            values.Select(x => new TestCaseData(x.Item1, x.Item2, x.Item3));

        public static IEnumerable<TestCaseData> ToTestCaseData<T1, T2>(this IEnumerable<ValueTuple<T1, T2>> values) =>
            values.Select(x => new TestCaseData(x.Item1, x.Item2));

        public static IEnumerable<TestCaseData> ToTestCaseData<T>(this IEnumerable<T> values) =>
            values.Select(x => new TestCaseData(x));
    }
}
