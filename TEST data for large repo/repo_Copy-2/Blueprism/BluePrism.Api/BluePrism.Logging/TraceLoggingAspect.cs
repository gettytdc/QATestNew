namespace BluePrism.Logging
{
    using System.Diagnostics;
    using Castle.DynamicProxy;
    using Func;

    public class TraceLoggingAspect : AsyncTimingInterceptor
    {
        private readonly ILoggerFactory _loggerFactory;

        public TraceLoggingAspect(ILoggerFactory loggerFactory)
        {
            _loggerFactory = loggerFactory;
        }

        protected override void StartingTiming(IInvocation invocation) =>
            GetLogger(invocation)
            .Trace("{method} starting", invocation.Method.Name);

        protected override void CompletedTiming(IInvocation invocation, Stopwatch stopwatch) =>
            GetLogger(invocation)
            .Trace("{method} completed in {elapsed}ms", invocation.Method.Name, stopwatch.ElapsedMilliseconds);

        private NLog.ILogger GetLogger(IInvocation invocation) =>
            invocation.TargetType.FullName
            .Map(_loggerFactory.GetLogger);
    }
}
