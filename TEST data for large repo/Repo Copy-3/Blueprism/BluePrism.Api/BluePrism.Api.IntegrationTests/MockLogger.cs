namespace BluePrism.Api.IntegrationTests
{
    using System;
    using System.Threading.Tasks;
    using Autofac.Extras.Moq;
    using Logging;

    public class MockLogger<T> : ILogger<T>
    {
        private readonly AutoMock _autoMock;

        private ILogger<T> Mock => _autoMock.Mock<ILogger<T>>().Object;


        public MockLogger(AutoMock autoMock)
        {
            _autoMock = autoMock;
        }


        public bool IsTraceEnabled => Mock.IsTraceEnabled;
        public bool IsDebugEnabled => Mock.IsDebugEnabled;
        public bool IsInfoEnabled => Mock.IsInfoEnabled;
        public bool IsWarnEnabled => Mock.IsWarnEnabled;
        public bool IsErrorEnabled => Mock.IsErrorEnabled;
        public bool IsFatalEnabled => Mock.IsFatalEnabled;
        public string Name => Mock.Name;
        public void Debug(object value) => Mock.Debug(value);
        public void Debug(IFormatProvider formatProvider, object value) => Mock.Debug(formatProvider, value);
        public void Debug(string message, object arg1, object arg2) => Mock.Debug(message, arg1, arg2);
        public void Debug(string message, object arg1, object arg2, object arg3) => Mock.Debug(message, arg1, arg2, arg3);
        public void Debug(IFormatProvider formatProvider, string message, bool argument) => Mock.Debug(formatProvider, message, argument);
        public void Debug(string message, bool argument) => Mock.Debug(message, argument);
        public void Debug(IFormatProvider formatProvider, string message, char argument) => Mock.Debug(formatProvider, message, argument);
        public void Debug(string message, char argument) => Mock.Debug(message, argument);
        public void Debug(IFormatProvider formatProvider, string message, byte argument) => Mock.Debug(formatProvider, message, argument);
        public void Debug(string message, byte argument) => Mock.Debug(message, argument);
        public void Debug(IFormatProvider formatProvider, string message, string argument) => Mock.Debug(formatProvider, message, argument);
        public void Debug(string message, string argument) => Mock.Debug(message, argument);
        public void Debug(IFormatProvider formatProvider, string message, int argument) => Mock.Debug(formatProvider, message, argument);
        public void Debug(string message, int argument) => Mock.Debug(message, argument);
        public void Debug(IFormatProvider formatProvider, string message, long argument) => Mock.Debug(formatProvider, message, argument);
        public void Debug(string message, long argument) => Mock.Debug(message, argument);
        public void Debug(IFormatProvider formatProvider, string message, float argument) => Mock.Debug(formatProvider, message, argument);
        public void Debug(string message, float argument) => Mock.Debug(message, argument);
        public void Debug(IFormatProvider formatProvider, string message, double argument) => Mock.Debug(formatProvider, message, argument);
        public void Debug(string message, double argument) => Mock.Debug(message, argument);
        public void Debug(IFormatProvider formatProvider, string message, decimal argument) => Mock.Debug(formatProvider, message, argument);
        public void Debug(string message, decimal argument) => Mock.Debug(message, argument);
        public void Debug(IFormatProvider formatProvider, string message, object argument) => Mock.Debug(formatProvider, message, argument);
        public void Debug(string message, object argument) => Mock.Debug(message, argument);
        public void Debug(IFormatProvider formatProvider, string message, sbyte argument) => Mock.Debug(formatProvider, message, argument);
        public void Debug(string message, sbyte argument) => Mock.Debug(message, argument);
        public void Debug(IFormatProvider formatProvider, string message, uint argument) => Mock.Debug(formatProvider, message, argument);
        public void Debug(string message, uint argument) => Mock.Debug(message, argument);
        public void Debug(IFormatProvider formatProvider, string message, ulong argument) => Mock.Debug(formatProvider, message, argument);
        public void Debug(string message, ulong argument) => Mock.Debug(message, argument);
        public void Debug<T1>(T1 value) => Mock.Debug<T1>(value);
        public void Debug<T1>(IFormatProvider formatProvider, T1 value) => Mock.Debug<T1>(formatProvider, value);
        public void Debug(LogMessageGenerator messageFunc) => Mock.Debug(messageFunc);
        public void Debug(Exception exception, string message) => Mock.Debug(exception, message);
        public void Debug(Exception exception, string message, params object[] args) => Mock.Debug(exception, message, args);
        public void Debug(Exception exception, IFormatProvider formatProvider, string message, params object[] args) => Mock.Debug(exception, formatProvider, message, args);
        public void Debug(IFormatProvider formatProvider, string message, params object[] args) => Mock.Debug(formatProvider, message, args);
        public void Debug(string message) => Mock.Debug(message);
        public void Debug(string message, params object[] args) => Mock.Debug(message, args);
        public void Debug(string message, Exception exception) => Mock.Debug(message, exception);
        public void Debug<TArgument>(IFormatProvider formatProvider, string message, TArgument argument) => Mock.Debug<TArgument>(formatProvider, message, argument);
        public void Debug<TArgument>(string message, TArgument argument) => Mock.Debug<TArgument>(message, argument);

        public void Debug<TArgument1, TArgument2>(IFormatProvider formatProvider, string message, TArgument1 argument1,
            TArgument2 argument2) => Mock.Debug<TArgument1, TArgument2>(formatProvider, message, argument1, argument2);

        public void Debug<TArgument1, TArgument2>(string message, TArgument1 argument1, TArgument2 argument2) =>
            Mock.Debug<TArgument1, TArgument2>(message, argument1, argument2);

        public void Debug<TArgument1, TArgument2, TArgument3>(IFormatProvider formatProvider, string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3) =>
            Mock.Debug<TArgument1, TArgument2, TArgument3>(formatProvider, message, argument1, argument2, argument3);

        public void Debug<TArgument1, TArgument2, TArgument3>(string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3) =>
            Mock.Debug<TArgument1, TArgument2, TArgument3>(message, argument1, argument2, argument3);

        public void DebugException(string message, Exception exception) => Mock.DebugException(message, exception);
        public void Error(object value) => Mock.Error(value);
        public void Error(IFormatProvider formatProvider, object value) => Mock.Error(formatProvider, value);
        public void Error(string message, object arg1, object arg2) => Mock.Error(message, arg1, arg2);
        public void Error(string message, object arg1, object arg2, object arg3) => Mock.Error(message, arg1, arg2, arg3);
        public void Error(IFormatProvider formatProvider, string message, bool argument) => Mock.Error(formatProvider, message, argument);
        public void Error(string message, bool argument) => Mock.Error(message, argument);
        public void Error(IFormatProvider formatProvider, string message, char argument) => Mock.Error(formatProvider, message, argument);
        public void Error(string message, char argument) => Mock.Error(message, argument);
        public void Error(IFormatProvider formatProvider, string message, byte argument) => Mock.Error(formatProvider, message, argument);
        public void Error(string message, byte argument) => Mock.Error(message, argument);
        public void Error(IFormatProvider formatProvider, string message, string argument) => Mock.Error(formatProvider, message, argument);
        public void Error(string message, string argument) => Mock.Error(message, argument);
        public void Error(IFormatProvider formatProvider, string message, int argument) => Mock.Error(formatProvider, message, argument);
        public void Error(string message, int argument) => Mock.Error(message, argument);
        public void Error(IFormatProvider formatProvider, string message, long argument) => Mock.Error(formatProvider, message, argument);
        public void Error(string message, long argument) => Mock.Error(message, argument);
        public void Error(IFormatProvider formatProvider, string message, float argument) => Mock.Error(formatProvider, message, argument);
        public void Error(string message, float argument) => Mock.Error(message, argument);
        public void Error(IFormatProvider formatProvider, string message, double argument) => Mock.Error(formatProvider, message, argument);
        public void Error(string message, double argument) => Mock.Error(message, argument);
        public void Error(IFormatProvider formatProvider, string message, decimal argument) => Mock.Error(formatProvider, message, argument);
        public void Error(string message, decimal argument) => Mock.Error(message, argument);
        public void Error(IFormatProvider formatProvider, string message, object argument) => Mock.Error(formatProvider, message, argument);
        public void Error(string message, object argument) => Mock.Error(message, argument);
        public void Error(IFormatProvider formatProvider, string message, sbyte argument) => Mock.Error(formatProvider, message, argument);
        public void Error(string message, sbyte argument) => Mock.Error(message, argument);
        public void Error(IFormatProvider formatProvider, string message, uint argument) => Mock.Error(formatProvider, message, argument);
        public void Error(string message, uint argument) => Mock.Error(message, argument);
        public void Error(IFormatProvider formatProvider, string message, ulong argument) => Mock.Error(formatProvider, message, argument);
        public void Error(string message, ulong argument) => Mock.Error(message, argument);
        public void Error<T1>(T1 value) => Mock.Error<T1>(value);
        public void Error<T1>(IFormatProvider formatProvider, T1 value) => Mock.Error<T1>(formatProvider, value);
        public void Error(LogMessageGenerator messageFunc) => Mock.Error(messageFunc);
        public void Error(Exception exception, string message) => Mock.Error(exception, message);
        public void Error(Exception exception, string message, params object[] args) => Mock.Error(exception, message, args);
        public void Error(Exception exception, IFormatProvider formatProvider, string message, params object[] args) => Mock.Error(exception, formatProvider, message, args);
        public void Error(IFormatProvider formatProvider, string message, params object[] args) => Mock.Error(formatProvider, message, args);
        public void Error(string message) => Mock.Error(message);
        public void Error(string message, params object[] args) => Mock.Error(message, args);
        public void Error(string message, Exception exception) => Mock.Error(message, exception);
        public void Error<TArgument>(IFormatProvider formatProvider, string message, TArgument argument) => Mock.Error<TArgument>(formatProvider, message, argument);
        public void Error<TArgument>(string message, TArgument argument) => Mock.Error<TArgument>(message, argument);

        public void Error<TArgument1, TArgument2>(IFormatProvider formatProvider, string message, TArgument1 argument1, TArgument2 argument2)
            => Mock.Error<TArgument1, TArgument2>(formatProvider, message, argument1, argument2);

        public void Error<TArgument1, TArgument2>(string message, TArgument1 argument1, TArgument2 argument2) =>
            Mock.Error<TArgument1, TArgument2>(message, argument1, argument2);

        public void Error<TArgument1, TArgument2, TArgument3>(IFormatProvider formatProvider, string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3) =>
            Mock.Error<TArgument1, TArgument2, TArgument3>(formatProvider, message, argument1, argument2, argument3);

        public void Error<TArgument1, TArgument2, TArgument3>(string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3) =>
            Mock.Error<TArgument1, TArgument2, TArgument3>(message, argument1, argument2, argument3);

        public void ErrorException(string message, Exception exception) => Mock.ErrorException(message, exception);
        public void Fatal(object value) => Mock.Fatal(value);
        public void Fatal(IFormatProvider formatProvider, object value) => Mock.Fatal(formatProvider, value);
        public void Fatal(string message, object arg1, object arg2) => Mock.Fatal(message, arg1, arg2);
        public void Fatal(string message, object arg1, object arg2, object arg3) => Mock.Fatal(message, arg1, arg2, arg3);
        public void Fatal(IFormatProvider formatProvider, string message, bool argument) => Mock.Fatal(formatProvider, message, argument);
        public void Fatal(string message, bool argument) => Mock.Fatal(message, argument);
        public void Fatal(IFormatProvider formatProvider, string message, char argument) => Mock.Fatal(formatProvider, message, argument);
        public void Fatal(string message, char argument) => Mock.Fatal(message, argument);
        public void Fatal(IFormatProvider formatProvider, string message, byte argument) => Mock.Fatal(formatProvider, message, argument);
        public void Fatal(string message, byte argument) => Mock.Fatal(message, argument);
        public void Fatal(IFormatProvider formatProvider, string message, string argument) => Mock.Fatal(formatProvider, message, argument);
        public void Fatal(string message, string argument) => Mock.Fatal(message, argument);
        public void Fatal(IFormatProvider formatProvider, string message, int argument) => Mock.Fatal(formatProvider, message, argument);
        public void Fatal(string message, int argument) => Mock.Fatal(message, argument);
        public void Fatal(IFormatProvider formatProvider, string message, long argument) => Mock.Fatal(formatProvider, message, argument);
        public void Fatal(string message, long argument) => Mock.Fatal(message, argument);
        public void Fatal(IFormatProvider formatProvider, string message, float argument) => Mock.Fatal(formatProvider, message, argument);
        public void Fatal(string message, float argument) => Mock.Fatal(message, argument);
        public void Fatal(IFormatProvider formatProvider, string message, double argument) => Mock.Fatal(formatProvider, message, argument);
        public void Fatal(string message, double argument) => Mock.Fatal(message, argument);
        public void Fatal(IFormatProvider formatProvider, string message, decimal argument) => Mock.Fatal(formatProvider, message, argument);
        public void Fatal(string message, decimal argument) => Mock.Fatal(message, argument);
        public void Fatal(IFormatProvider formatProvider, string message, object argument) => Mock.Fatal(formatProvider, message, argument);
        public void Fatal(string message, object argument) => Mock.Fatal(message, argument);
        public void Fatal(IFormatProvider formatProvider, string message, sbyte argument) => Mock.Fatal(formatProvider, message, argument);
        public void Fatal(string message, sbyte argument) => Mock.Fatal(message, argument);
        public void Fatal(IFormatProvider formatProvider, string message, uint argument) => Mock.Fatal(formatProvider, message, argument);
        public void Fatal(string message, uint argument) => Mock.Fatal(message, argument);
        public void Fatal(IFormatProvider formatProvider, string message, ulong argument) => Mock.Fatal(formatProvider, message, argument);
        public void Fatal(string message, ulong argument) => Mock.Fatal(message, argument);
        public void Fatal<T1>(T1 value) => Mock.Fatal<T1>(value);
        public void Fatal<T1>(IFormatProvider formatProvider, T1 value) => Mock.Fatal<T1>(formatProvider, value);
        public void Fatal(LogMessageGenerator messageFunc) => Mock.Fatal(messageFunc);
        public void Fatal(Exception exception, string message) => Mock.Fatal(exception, message);
        public void Fatal(Exception exception, string message, params object[] args) => Mock.Fatal(exception, message, args);
        public void Fatal(Exception exception, IFormatProvider formatProvider, string message, params object[] args) => Mock.Fatal(exception, formatProvider, message, args);
        public void Fatal(IFormatProvider formatProvider, string message, params object[] args) => Mock.Fatal(formatProvider, message, args);
        public void Fatal(string message) => Mock.Fatal(message);
        public void Fatal(string message, params object[] args) => Mock.Fatal(message, args);
        public void Fatal(string message, Exception exception) => Mock.Fatal(message, exception);
        public void Fatal<TArgument>(IFormatProvider formatProvider, string message, TArgument argument) => Mock.Fatal<TArgument>(formatProvider, message, argument);
        public void Fatal<TArgument>(string message, TArgument argument) => Mock.Fatal<TArgument>(message, argument);

        public void Fatal<TArgument1, TArgument2>(IFormatProvider formatProvider, string message, TArgument1 argument1, TArgument2 argument2) =>
            Mock.Fatal<TArgument1, TArgument2>(formatProvider, message, argument1, argument2);

        public void Fatal<TArgument1, TArgument2>(string message, TArgument1 argument1, TArgument2 argument2) =>
            Mock.Fatal<TArgument1, TArgument2>(message, argument1, argument2);

        public void Fatal<TArgument1, TArgument2, TArgument3>(IFormatProvider formatProvider, string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3) =>
            Mock.Fatal<TArgument1, TArgument2, TArgument3>(formatProvider, message, argument1, argument2, argument3);

        public void Fatal<TArgument1, TArgument2, TArgument3>(string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3) =>
            Mock.Fatal<TArgument1, TArgument2, TArgument3>(message, argument1, argument2, argument3);

        public void FatalException(string message, Exception exception) => Mock.FatalException(message, exception);
        public void Info(object value) => Mock.Info(value);
        public void Info(IFormatProvider formatProvider, object value) => Mock.Info(formatProvider, value);
        public void Info(string message, object arg1, object arg2) => Mock.Info(message, arg1, arg2);
        public void Info(string message, object arg1, object arg2, object arg3) => Mock.Info(message, arg1, arg2, arg3);
        public void Info(IFormatProvider formatProvider, string message, bool argument) => Mock.Info(formatProvider, message, argument);
        public void Info(string message, bool argument) => Mock.Info(message, argument);
        public void Info(IFormatProvider formatProvider, string message, char argument) => Mock.Info(formatProvider, message, argument);
        public void Info(string message, char argument) => Mock.Info(message, argument);
        public void Info(IFormatProvider formatProvider, string message, byte argument) => Mock.Info(formatProvider, message, argument);
        public void Info(string message, byte argument) => Mock.Info(message, argument);
        public void Info(IFormatProvider formatProvider, string message, string argument) => Mock.Info(formatProvider, message, argument);
        public void Info(string message, string argument) => Mock.Info(message, argument);
        public void Info(IFormatProvider formatProvider, string message, int argument) => Mock.Info(formatProvider, message, argument);
        public void Info(string message, int argument) => Mock.Info(message, argument);
        public void Info(IFormatProvider formatProvider, string message, long argument) => Mock.Info(formatProvider, message, argument);
        public void Info(string message, long argument) => Mock.Info(message, argument);
        public void Info(IFormatProvider formatProvider, string message, float argument) => Mock.Info(formatProvider, message, argument);
        public void Info(string message, float argument) => Mock.Info(message, argument);
        public void Info(IFormatProvider formatProvider, string message, double argument) => Mock.Info(formatProvider, message, argument);
        public void Info(string message, double argument) => Mock.Info(message, argument);
        public void Info(IFormatProvider formatProvider, string message, decimal argument) => Mock.Info(formatProvider, message, argument);
        public void Info(string message, decimal argument) => Mock.Info(message, argument);
        public void Info(IFormatProvider formatProvider, string message, object argument) => Mock.Info(formatProvider, message, argument);
        public void Info(string message, object argument) => Mock.Info(message, argument);
        public void Info(IFormatProvider formatProvider, string message, sbyte argument) => Mock.Info(formatProvider, message, argument);
        public void Info(string message, sbyte argument) => Mock.Info(message, argument);
        public void Info(IFormatProvider formatProvider, string message, uint argument) => Mock.Info(formatProvider, message, argument);
        public void Info(string message, uint argument) => Mock.Info(message, argument);
        public void Info(IFormatProvider formatProvider, string message, ulong argument) => Mock.Info(formatProvider, message, argument);
        public void Info(string message, ulong argument) => Mock.Info(message, argument);
        public void Info<T1>(T1 value) => Mock.Info<T1>(value);
        public void Info<T1>(IFormatProvider formatProvider, T1 value) => Mock.Info<T1>(formatProvider, value);
        public void Info(LogMessageGenerator messageFunc) => Mock.Info(messageFunc);
        public void Info(Exception exception, string message) => Mock.Info(exception, message);
        public void Info(Exception exception, string message, params object[] args) => Mock.Info(exception, message, args);
        public void Info(Exception exception, IFormatProvider formatProvider, string message, params object[] args) => Mock.Info(exception, formatProvider, message, args);
        public void Info(IFormatProvider formatProvider, string message, params object[] args) => Mock.Info(formatProvider, message, args);
        public void Info(string message) => Mock.Info(message);
        public void Info(string message, params object[] args) => Mock.Info(message, args);
        public void Info(string message, Exception exception) => Mock.Info(message, exception);
        public void Info<TArgument>(IFormatProvider formatProvider, string message, TArgument argument) => Mock.Info<TArgument>(formatProvider, message, argument);
        public void Info<TArgument>(string message, TArgument argument) => Mock.Info<TArgument>(message, argument);

        public void Info<TArgument1, TArgument2>(IFormatProvider formatProvider, string message, TArgument1 argument1, TArgument2 argument2) =>
            Mock.Info<TArgument1, TArgument2>(formatProvider, message, argument1, argument2);

        public void Info<TArgument1, TArgument2>(string message, TArgument1 argument1, TArgument2 argument2) =>
            Mock.Info<TArgument1, TArgument2>(message, argument1, argument2);

        public void Info<TArgument1, TArgument2, TArgument3>(IFormatProvider formatProvider, string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3) =>
            Mock.Info<TArgument1, TArgument2, TArgument3>(formatProvider, message, argument1, argument2, argument3);

        public void Info<TArgument1, TArgument2, TArgument3>(string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3) =>
            Mock.Info<TArgument1, TArgument2, TArgument3>(message, argument1, argument2, argument3);

        public void InfoException(string message, Exception exception) => Mock.InfoException(message, exception);
        public bool IsEnabled(Logging.LogLevel level) => Mock.IsEnabled(level);
        public void Log(Logging.LogLevel level, object value) => Mock.Log(level, value);
        public void Log(Logging.LogLevel level, IFormatProvider formatProvider, object value) => Mock.Log(level, formatProvider, value);
        public void Log(Logging.LogLevel level, string message, object arg1, object arg2) => Mock.Log(level, message, arg1, arg2);
        public void Log(Logging.LogLevel level, string message, object arg1, object arg2, object arg3) => Mock.Log(level, message, arg1, arg2, arg3);
        public void Log(Logging.LogLevel level, IFormatProvider formatProvider, string message, bool argument) => Mock.Log(level, formatProvider, message, argument);
        public void Log(Logging.LogLevel level, string message, bool argument) => Mock.Log(level, message, argument);
        public void Log(Logging.LogLevel level, IFormatProvider formatProvider, string message, char argument) => Mock.Log(level, formatProvider, message, argument);
        public void Log(Logging.LogLevel level, string message, char argument) => Mock.Log(level, message, argument);
        public void Log(Logging.LogLevel level, IFormatProvider formatProvider, string message, byte argument) => Mock.Log(level, formatProvider, message, argument);
        public void Log(Logging.LogLevel level, string message, byte argument) => Mock.Log(level, message, argument);
        public void Log(Logging.LogLevel level, IFormatProvider formatProvider, string message, string argument) => Mock.Log(level, formatProvider, message, argument);
        public void Log(Logging.LogLevel level, string message, string argument) => Mock.Log(level, message, argument);
        public void Log(Logging.LogLevel level, IFormatProvider formatProvider, string message, int argument) => Mock.Log(level, formatProvider, message, argument);
        public void Log(Logging.LogLevel level, string message, int argument) => Mock.Log(level, message, argument);
        public void Log(Logging.LogLevel level, IFormatProvider formatProvider, string message, long argument) => Mock.Log(level, formatProvider, message, argument);
        public void Log(Logging.LogLevel level, string message, long argument) => Mock.Log(level, message, argument);
        public void Log(Logging.LogLevel level, IFormatProvider formatProvider, string message, float argument) => Mock.Log(level, formatProvider, message, argument);
        public void Log(Logging.LogLevel level, string message, float argument) => Mock.Log(level, message, argument);
        public void Log(Logging.LogLevel level, IFormatProvider formatProvider, string message, double argument) => Mock.Log(level, formatProvider, message, argument);
        public void Log(Logging.LogLevel level, string message, double argument) => Mock.Log(level, message, argument);
        public void Log(Logging.LogLevel level, IFormatProvider formatProvider, string message, decimal argument) => Mock.Log(level, formatProvider, message, argument);
        public void Log(Logging.LogLevel level, string message, decimal argument) => Mock.Log(level, message, argument);
        public void Log(Logging.LogLevel level, IFormatProvider formatProvider, string message, object argument) => Mock.Log(level, formatProvider, message, argument);
        public void Log(Logging.LogLevel level, string message, object argument) => Mock.Log(level, message, argument);
        public void Log(Logging.LogLevel level, IFormatProvider formatProvider, string message, sbyte argument) => Mock.Log(level, formatProvider, message, argument);
        public void Log(Logging.LogLevel level, string message, sbyte argument) => Mock.Log(level, message, argument);
        public void Log(Logging.LogLevel level, IFormatProvider formatProvider, string message, uint argument) => Mock.Log(level, formatProvider, message, argument);
        public void Log(Logging.LogLevel level, string message, uint argument) => Mock.Log(level, message, argument);
        public void Log(Logging.LogLevel level, IFormatProvider formatProvider, string message, ulong argument) => Mock.Log(level, formatProvider, message, argument);
        public void Log(Logging.LogLevel level, string message, ulong argument) => Mock.Log(level, message, argument);
        public void Log<T1>(Logging.LogLevel level, T1 value) => Mock.Log<T1>(level, value);
        public void Log<T1>(Logging.LogLevel level, IFormatProvider formatProvider, T1 value) => Mock.Log<T1>(level, formatProvider, value);
        public void Log(Logging.LogLevel level, LogMessageGenerator messageFunc) => Mock.Log(level, messageFunc);
        public void Log(Logging.LogLevel level, Exception exception, string message, params object[] args) => Mock.Log(level, exception, message, args);
        public void Log(Logging.LogLevel level, Exception exception, IFormatProvider formatProvider, string message, params object[] args) => Mock.Log(level, exception, formatProvider, message, args);
        public void Log(Logging.LogLevel level, IFormatProvider formatProvider, string message, params object[] args) => Mock.Log(level, formatProvider, message, args);
        public void Log(Logging.LogLevel level, string message) => Mock.Log(level, message);
        public void Log(Logging.LogLevel level, string message, params object[] args) => Mock.Log(level, message, args);
        public void Log(Logging.LogLevel level, string message, Exception exception) => Mock.Log(level, message, exception);
        public void Log<TArgument>(Logging.LogLevel level, IFormatProvider formatProvider, string message, TArgument argument) => Mock.Log<TArgument>(level, formatProvider, message, argument);
        public void Log<TArgument>(Logging.LogLevel level, string message, TArgument argument) => Mock.Log<TArgument>(level, message, argument);

        public void Log<TArgument1, TArgument2>(Logging.LogLevel level, IFormatProvider formatProvider, string message, TArgument1 argument1, TArgument2 argument2) =>
            Mock.Log<TArgument1, TArgument2>(level, formatProvider, message, argument1, argument2);

        public void Log<TArgument1, TArgument2>(Logging.LogLevel level, string message, TArgument1 argument1, TArgument2 argument2) =>
            Mock.Log<TArgument1, TArgument2>(level, message, argument1, argument2);

        public void Log<TArgument1, TArgument2, TArgument3>(Logging.LogLevel level, IFormatProvider formatProvider, string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3) =>
            Mock.Log<TArgument1, TArgument2, TArgument3>(level, formatProvider, message, argument1, argument2, argument3);

        public void Log<TArgument1, TArgument2, TArgument3>(Logging.LogLevel level, string message, TArgument1 argument1, TArgument2 argument2, TArgument3 argument3) =>
            Mock.Log<TArgument1, TArgument2, TArgument3>(level, message, argument1, argument2, argument3);

        public void LogException(Logging.LogLevel level, string message, Exception exception) => Mock.LogException(level, message, exception);
        public void Swallow(Action action) => Mock.Swallow(action);
        public T1 Swallow<T1>(Func<T1> func) => Mock.Swallow<T1>(func);
        public T1 Swallow<T1>(Func<T1> func, T1 fallback) => Mock.Swallow<T1>(func, fallback);
        public void Swallow(Task task) => Mock.Swallow(task);
        public Task SwallowAsync(Task task) => Mock.SwallowAsync(task);
        public Task SwallowAsync(Func<Task> asyncAction) => Mock.SwallowAsync(asyncAction);
        public Task<TResult> SwallowAsync<TResult>(Func<Task<TResult>> asyncFunc) => Mock.SwallowAsync<TResult>(asyncFunc);
        public Task<TResult> SwallowAsync<TResult>(Func<Task<TResult>> asyncFunc, TResult fallback) => Mock.SwallowAsync<TResult>(asyncFunc, fallback);
        public void Trace(object value) => Mock.Trace(value);
        public void Trace(IFormatProvider formatProvider, object value) => Mock.Trace(formatProvider, value);
        public void Trace(string message, object arg1, object arg2) => Mock.Trace(message, arg1, arg2);
        public void Trace(string message, object arg1, object arg2, object arg3) => Mock.Trace(message, arg1, arg2, arg3);
        public void Trace(IFormatProvider formatProvider, string message, bool argument) => Mock.Trace(formatProvider, message, argument);
        public void Trace(string message, bool argument) => Mock.Trace(message, argument);
        public void Trace(IFormatProvider formatProvider, string message, char argument) => Mock.Trace(formatProvider, message, argument);
        public void Trace(string message, char argument) => Mock.Trace(message, argument);
        public void Trace(IFormatProvider formatProvider, string message, byte argument) => Mock.Trace(formatProvider, message, argument);
        public void Trace(string message, byte argument) => Mock.Trace(message, argument);

        public void Trace(IFormatProvider formatProvider, string message, string argument) => Mock.Trace(formatProvider, message, argument);

        public void Trace(string message, string argument) => Mock.Trace(message, argument);

        public void Trace(IFormatProvider formatProvider, string message, int argument) => Mock.Trace(formatProvider, message, argument);

        public void Trace(string message, int argument) => Mock.Trace(message, argument);

        public void Trace(IFormatProvider formatProvider, string message, long argument) => Mock.Trace(formatProvider, message, argument);

        public void Trace(string message, long argument) => Mock.Trace(message, argument);

        public void Trace(IFormatProvider formatProvider, string message, float argument) => Mock.Trace(formatProvider, message, argument);

        public void Trace(string message, float argument) => Mock.Trace(message, argument);

        public void Trace(IFormatProvider formatProvider, string message, double argument) => Mock.Trace(formatProvider, message, argument);

        public void Trace(string message, double argument) => Mock.Trace(message, argument);

        public void Trace(IFormatProvider formatProvider, string message, decimal argument) => Mock.Trace(formatProvider, message, argument);

        public void Trace(string message, decimal argument) => Mock.Trace(message, argument);

        public void Trace(IFormatProvider formatProvider, string message, object argument) => Mock.Trace(formatProvider, message, argument);

        public void Trace(string message, object argument) => Mock.Trace(message, argument);

        public void Trace(IFormatProvider formatProvider, string message, sbyte argument) => Mock.Trace(formatProvider, message, argument);

        public void Trace(string message, sbyte argument) => Mock.Trace(message, argument);

        public void Trace(IFormatProvider formatProvider, string message, uint argument) => Mock.Trace(formatProvider, message, argument);

        public void Trace(string message, uint argument) => Mock.Trace(message, argument);

        public void Trace(IFormatProvider formatProvider, string message, ulong argument) => Mock.Trace(formatProvider, message, argument);

        public void Trace(string message, ulong argument) => Mock.Trace(message, argument);

        public void Trace<T1>(T1 value) => Mock.Trace<T1>(value);

        public void Trace<T1>(IFormatProvider formatProvider, T1 value) => Mock.Trace<T1>(formatProvider, value);

        public void Trace(LogMessageGenerator messageFunc) => Mock.Trace(messageFunc);

        public void Trace(Exception exception, string message) => Mock.Trace(exception, message);

        public void Trace(Exception exception, string message, params object[] args) => Mock.Trace(exception, message, args);

        public void Trace(Exception exception, IFormatProvider formatProvider, string message, params object[] args) => Mock.Trace(exception, formatProvider, message, args);

        public void Trace(IFormatProvider formatProvider, string message, params object[] args) => Mock.Trace(formatProvider, message, args);

        public void Trace(string message) => Mock.Trace(message);

        public void Trace(string message, params object[] args) => Mock.Trace(message, args);

        public void Trace(string message, Exception exception) => Mock.Trace(message, exception);

        public void Trace<TArgument>(IFormatProvider formatProvider, string message, TArgument argument) => Mock.Trace<TArgument>(formatProvider, message, argument);

        public void Trace<TArgument>(string message, TArgument argument) => Mock.Trace<TArgument>(message, argument);

        public void Trace<TArgument1, TArgument2>(IFormatProvider formatProvider, string message, TArgument1 argument1,
            TArgument2 argument2) =>
            throw new NotImplementedException();

        public void Trace<TArgument1, TArgument2>(string message, TArgument1 argument1, TArgument2 argument2) => Mock.Trace<TArgument1, TArgument2>(message, argument1, argument2);

        public void Trace<TArgument1, TArgument2, TArgument3>(IFormatProvider formatProvider, string message, TArgument1 argument1,
            TArgument2 argument2, TArgument3 argument3) =>
            throw new NotImplementedException();

        public void Trace<TArgument1, TArgument2, TArgument3>(string message, TArgument1 argument1, TArgument2 argument2,
            TArgument3 argument3) =>
            throw new NotImplementedException();

        public void TraceException(string message, Exception exception) => Mock.TraceException(message, exception);

        public void Warn(object value) => Mock.Warn(value);

        public void Warn(IFormatProvider formatProvider, object value) => Mock.Warn(formatProvider, value);

        public void Warn(string message, object arg1, object arg2) => Mock.Warn(message, arg1, arg2);

        public void Warn(string message, object arg1, object arg2, object arg3) => Mock.Warn(message, arg1, arg2, arg3);

        public void Warn(IFormatProvider formatProvider, string message, bool argument) => Mock.Warn(formatProvider, message, argument);

        public void Warn(string message, bool argument) => Mock.Warn(message, argument);

        public void Warn(IFormatProvider formatProvider, string message, char argument) => Mock.Warn(formatProvider, message, argument);

        public void Warn(string message, char argument) => Mock.Warn(message, argument);

        public void Warn(IFormatProvider formatProvider, string message, byte argument) => Mock.Warn(formatProvider, message, argument);

        public void Warn(string message, byte argument) => Mock.Warn(message, argument);

        public void Warn(IFormatProvider formatProvider, string message, string argument) => Mock.Warn(formatProvider, message, argument);

        public void Warn(string message, string argument) => Mock.Warn(message, argument);

        public void Warn(IFormatProvider formatProvider, string message, int argument) => Mock.Warn(formatProvider, message, argument);

        public void Warn(string message, int argument) => Mock.Warn(message, argument);

        public void Warn(IFormatProvider formatProvider, string message, long argument) => Mock.Warn(formatProvider, message, argument);

        public void Warn(string message, long argument) => Mock.Warn(message, argument);

        public void Warn(IFormatProvider formatProvider, string message, float argument) => Mock.Warn(formatProvider, message, argument);

        public void Warn(string message, float argument) => Mock.Warn(message, argument);

        public void Warn(IFormatProvider formatProvider, string message, double argument) => Mock.Warn(formatProvider, message, argument);

        public void Warn(string message, double argument) => Mock.Warn(message, argument);

        public void Warn(IFormatProvider formatProvider, string message, decimal argument) => Mock.Warn(formatProvider, message, argument);

        public void Warn(string message, decimal argument) => Mock.Warn(message, argument);

        public void Warn(IFormatProvider formatProvider, string message, object argument) => Mock.Warn(formatProvider, message, argument);

        public void Warn(string message, object argument) => Mock.Warn(message, argument);

        public void Warn(IFormatProvider formatProvider, string message, sbyte argument) => Mock.Warn(formatProvider, message, argument);

        public void Warn(string message, sbyte argument) => Mock.Warn(message, argument);

        public void Warn(IFormatProvider formatProvider, string message, uint argument) => Mock.Warn(formatProvider, message, argument);

        public void Warn(string message, uint argument) => Mock.Warn(message, argument);

        public void Warn(IFormatProvider formatProvider, string message, ulong argument) => Mock.Warn(formatProvider, message, argument);

        public void Warn(string message, ulong argument) => Mock.Warn(message, argument);

        public void Warn<T1>(T1 value) => Mock.Warn<T1>(value);

        public void Warn<T1>(IFormatProvider formatProvider, T1 value) => Mock.Warn<T1>(formatProvider, value);

        public void Warn(LogMessageGenerator messageFunc) => Mock.Warn(messageFunc);

        public void Warn(Exception exception, string message) => Mock.Warn(exception, message);

        public void Warn(Exception exception, string message, params object[] args) => Mock.Warn(exception, message, args);

        public void Warn(Exception exception, IFormatProvider formatProvider, string message, params object[] args) => Mock.Warn(exception, formatProvider, message, args);

        public void Warn(IFormatProvider formatProvider, string message, params object[] args) => Mock.Warn(formatProvider, message, args);

        public void Warn(string message) => Mock.Warn(message);

        public void Warn(string message, params object[] args) => Mock.Warn(message, args);

        public void Warn(string message, Exception exception) => Mock.Warn(message, exception);

        public void Warn<TArgument>(IFormatProvider formatProvider, string message, TArgument argument) => Mock.Warn<TArgument>(formatProvider, message, argument);

        public void Warn<TArgument>(string message, TArgument argument) => Mock.Warn<TArgument>(message, argument);

        public void Warn<TArgument1, TArgument2>(IFormatProvider formatProvider, string message, TArgument1 argument1,
            TArgument2 argument2) =>
            throw new NotImplementedException();

        public void Warn<TArgument1, TArgument2>(string message, TArgument1 argument1, TArgument2 argument2) => Mock.Warn<TArgument1, TArgument2>(message, argument1, argument2);

        public void Warn<TArgument1, TArgument2, TArgument3>(IFormatProvider formatProvider, string message, TArgument1 argument1,
            TArgument2 argument2, TArgument3 argument3) =>
            throw new NotImplementedException();

        public void Warn<TArgument1, TArgument2, TArgument3>(string message, TArgument1 argument1, TArgument2 argument2,
            TArgument3 argument3) =>
            throw new NotImplementedException();

        public void WarnException(string message, Exception exception) => Mock.WarnException(message, exception);
    }
}
