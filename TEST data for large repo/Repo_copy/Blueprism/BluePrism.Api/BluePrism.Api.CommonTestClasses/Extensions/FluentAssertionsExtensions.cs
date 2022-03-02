namespace BluePrism.Api.CommonTestClasses.Extensions
{
    using FluentAssertions;

    public static class FluentAssertionsExtensions
    {
        public static void ShouldRuntimeTypesBeEquivalentTo<T>(this T subject, object expectation) =>
            subject.ShouldBeEquivalentTo(expectation, options => options.RespectingRuntimeTypes());
    }
}
