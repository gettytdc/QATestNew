namespace BluePrism.Api.CommonTestClasses
{
    using System;
    using System.Collections;
    using System.Linq;

    public class EnumTestCaseSource<TEnum> : IEnumerable
    {
        public IEnumerator GetEnumerator() =>
            Enum.GetValues(typeof(TEnum))
                .Cast<TEnum>()
                .GetEnumerator();
    }
}