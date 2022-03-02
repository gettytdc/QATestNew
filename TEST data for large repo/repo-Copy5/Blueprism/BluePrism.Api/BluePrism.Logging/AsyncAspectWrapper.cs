namespace BluePrism.Logging
{
    using Castle.DynamicProxy;

    public class AsyncAspectWrapper<TAspect> : IInterceptor where TAspect : IAsyncInterceptor
    {
        private readonly IInterceptor _wrappedAspect;

        public AsyncAspectWrapper(TAspect wrappedAspect)
        {
            _wrappedAspect = wrappedAspect.ToInterceptor();
        }

        public void Intercept(IInvocation invocation)
        {
            _wrappedAspect.Intercept(invocation);
        }
    }
}
