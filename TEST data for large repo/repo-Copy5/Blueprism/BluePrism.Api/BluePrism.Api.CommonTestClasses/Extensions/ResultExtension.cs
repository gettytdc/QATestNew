namespace BluePrism.Api.CommonTestClasses.Extensions
{
    using Func;

    public static class ResultExtension
    {
        public static Success<T> ToSuccess <T>(this Result<T> result) => (Success<T>)result;
    }
}
