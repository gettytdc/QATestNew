namespace BluePrism.UnitTesting.TestSupport
{
    using System;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Runtime.CompilerServices;
    using Utilities.Functional;

    public static class MethodReplacer
    {
        public static void ReplaceMethod(Expression<Action> replace, Expression<Action> replaceWith) =>
            ReplaceMethodFromLambdaExpressions(replace, replaceWith);

        public static void ReplaceMethod<TResult>(Expression<Func<TResult>> replace, Expression<Func<TResult>> replaceWith) =>
            ReplaceMethodFromLambdaExpressions(replace, replaceWith);

        public static void ReplaceMethod(MethodInfo replace, MethodInfo replaceWith)
        {
            RuntimeHelpers.PrepareMethod(replace.MethodHandle);
            RuntimeHelpers.PrepareMethod(replaceWith.MethodHandle);

            unsafe
            {
                if (IntPtr.Size == 4)
                {
                    var targetPointer = (UInt32*)replace.MethodHandle.Value.ToPointer() + 2;
                    var sourcePointer = (UInt32*)replaceWith.MethodHandle.Value.ToPointer() + 2;

                    *targetPointer = *sourcePointer;
                }
                else
                {
                    var targetPointer = (UInt64*)replace.MethodHandle.Value.ToPointer() + 1;
                    var sourcePointer = (UInt64*)replaceWith.MethodHandle.Value.ToPointer() + 1;

                    *targetPointer = *sourcePointer;
                }
            }
        }

        public static void ReplaceMethodFromLambdaExpressions(LambdaExpression replace, LambdaExpression replaceWith)
        {
            var replaceParameterTypes =
                (replace.Body as MethodCallExpression)
                .Map(GetParameterTypes);
            
            var replaceWithParameterTypes =
                (replaceWith.Body as MethodCallExpression)
                .Map(GetParameterTypes);

            if (!replaceParameterTypes.SequenceEqual(replaceWithParameterTypes))
                throw new ArgumentException();

            ReplaceMethod(
                (replace.Body as MethodCallExpression).Method,
                (replaceWith.Body as MethodCallExpression).Method);
        }

        private static Type[] GetParameterTypes(MethodCallExpression expression) =>
            expression
            .Arguments
            .Select(x => x.Type)
            .ToArray();
    }

    public static class MethodReplacer<TTarget, TSource>
    {
        public static void ReplaceMethod<TResult>(Expression<Func<TTarget, TResult>> replace, Expression<Func<TSource, TResult>> replaceWith) =>
            MethodReplacer.ReplaceMethodFromLambdaExpressions(replace, replaceWith);

        public static void ReplaceMethod(Expression<Action<TTarget>> replace, Expression<Action<TSource>> replaceWith) =>
            MethodReplacer.ReplaceMethodFromLambdaExpressions(replace, replaceWith);

        public static void ReplaceMethod(string replace, string replaceWith, Type[] parameterTypes) =>
            MethodReplacer.ReplaceMethod(
                GetMethod<TTarget>(replace, parameterTypes),
                GetMethod<TSource>(replaceWith, parameterTypes));

        private static MethodInfo GetMethod<T>(string name, Type[] parameterTypes) =>
            typeof(T).GetMethod(
                name,
                BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static | BindingFlags.Instance,
                null,
                parameterTypes,
                null);
    }
}
