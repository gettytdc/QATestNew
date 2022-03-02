namespace BluePrism.Api.CommonTestClasses.Extensions
{
    using FluentAssertions;
    using Logging;
    using Moq;

    public static class LoggingAssertionExtensions
    {
        public static void ShouldLogTraceMessages<TLoggerType>(this Mock<ILogger<TLoggerType>> loggerMock) =>
            loggerMock.ShouldLogMessagesOfType("Trace");

        public static void ShouldLogInfoMessages<TLoggerType>(this Mock<ILogger<TLoggerType>> loggerMock) =>
            loggerMock.ShouldLogMessagesOfType("Info");

        public static void ShouldLogDebugMessages<TLoggerType>(this Mock<ILogger<TLoggerType>> loggerMock) =>
            loggerMock.ShouldLogMessagesOfType("Debug");

        public static void ShouldLogWarnMessages<TLoggerType>(this Mock<ILogger<TLoggerType>> loggerMock) =>
            loggerMock.ShouldLogMessagesOfType("Warn");

        public static void ShouldLogErrorMessages<TLoggerType>(this Mock<ILogger<TLoggerType>> loggerMock) =>
            loggerMock.ShouldLogMessagesOfType("Error");

        public static void ShouldLogFatalMessages<TLoggerType>(this Mock<ILogger<TLoggerType>> loggerMock) =>
            loggerMock.ShouldLogMessagesOfType("Fatal");

        private static void ShouldLogMessagesOfType<TLoggerType>(this Mock<ILogger<TLoggerType>> loggerMock, string logMethodName) =>
            loggerMock.Invocations.Should().Contain(x => x.Method.Name == logMethodName);
    }
}
