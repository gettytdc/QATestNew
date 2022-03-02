namespace BluePrism.Api.BpLibAdapters
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading.Tasks;
    using Domain.Exceptions;
    using Func;
    using static Func.ResultHelper;
    using static Func.OptionHelper;

    public class ServerResultTask
    {
        private readonly Action _method;
        private readonly IReadOnlyDictionary<Type, Func<Exception, Result>> _catchMethods;

        internal ServerResultTask(Action method, IReadOnlyDictionary<Type, Func<Exception, Result>> catchMethods)
        {
            _catchMethods = catchMethods;
            _method = method;
        }

        public static implicit operator Task<Result>(ServerResultTask result) => result.Execute();

        public static ServerResultTask RunOnServer(Action method) =>
            new ServerResultTask(method, new Dictionary<Type, Func<Exception, Result>>());

        public static ServerResultTask<TValue> RunOnServer<TValue>(Func<TValue> method) =>
            new ServerResultTask<TValue>(method, new Dictionary<Type, Func<Exception, Result<TValue>>>(), None<Func<Result<TValue>>>());

        public ServerResultTask Catch<TException>(Func<TException, Result> catchMethod)
            where TException : Exception
            =>
            new ServerResultTask(_method, GetCatchMethodsWithAddedCatch(typeof(TException), GetNonGenericCatchMethod(catchMethod)));

        public Task<Result> Execute() => Task.Run(() =>
        {
            try
            {
                _method();
                return Succeed();
            }
            catch (Exception ex)
            {
                var exceptionType = ex.GetType();
                if (!_catchMethods.ContainsKey(exceptionType))
                    throw;

                return _catchMethods[exceptionType](ex);
            }
        });

        private IReadOnlyDictionary<Type, Func<Exception, Result>> GetCatchMethodsWithAddedCatch(
            Type exceptionType,
            Func<Exception, Result> catchMethod)
        {
            var copiedDictionary = _catchMethods.ToDictionary(x => x.Key, x => x.Value);
            copiedDictionary[exceptionType] = catchMethod;
            return copiedDictionary;
        }

        private static Func<Exception, Result> GetNonGenericCatchMethod<TException>(Func<TException, Result> catchMethod)
            where TException : Exception
            => ex => (ex as TException)?.Map(catchMethod) ?? throw new ArgumentException("Internal error. Either the exception was not of the expected type or the catch did not return a value");
    }

    public class ServerResultTask<TValue>
    {
        private readonly Func<TValue> _valueFactory;
        private readonly IReadOnlyDictionary<Type, Func<Exception, Result<TValue>>> _catchMethods;
        private readonly Option<Func<Result<TValue>>> _onNullMethod;

        internal ServerResultTask(
            Func<TValue> valueFactory,
            IReadOnlyDictionary<Type, Func<Exception, Result<TValue>>> catchMethods,
            Option<Func<Result<TValue>>> onNullMethod)
        {
            _valueFactory = valueFactory;
            _catchMethods = catchMethods;
            _onNullMethod = onNullMethod;
        }

        public static implicit operator Task<Result<TValue>>(ServerResultTask<TValue> result) => result.Execute();

        public ServerResultTask<TValue> Catch<TException>(Func<TException, Result<TValue>> catchMethod)
            where TException : Exception
            =>
            new ServerResultTask<TValue>(
                _valueFactory,
                GetCatchMethodsWithAddedCatch(
                        typeof(TException),
                        GetNonGenericCatchMethod(catchMethod)),
                _onNullMethod);

        public ServerResultTask<TValue> OnNull(Func<Result<TValue>> onNullMethod)
        {
            if (_onNullMethod is Some)
                throw new InvalidStateException("An OnNull method has already been assigned to this result");

            return
                new ServerResultTask<TValue>(
                    _valueFactory,
                    _catchMethods,
                    Some(onNullMethod));
        }

        public Task<Result<TValue>> Execute() => Task.Run(() =>
        {
            try
            {
                var value = _valueFactory();
                if (value == null && _onNullMethod is Some<Func<Result<TValue>>> onNullMethod)
                    return onNullMethod.Value();

                return Succeed(value);
            }
            catch (Exception ex)
            {
                var exceptionType = ex.GetType();
                if (!_catchMethods.ContainsKey(exceptionType))
                    throw;

                return _catchMethods[exceptionType](ex);
            }
        });

        private IReadOnlyDictionary<Type, Func<Exception, Result<TValue>>> GetCatchMethodsWithAddedCatch(
            Type exceptionType,
            Func<Exception, Result<TValue>> catchMethod)
        {
            var copiedDictionary = _catchMethods.ToDictionary(x => x.Key, x => x.Value);
            copiedDictionary[exceptionType] = catchMethod;
            return copiedDictionary;
        }

        private static Func<Exception, Result<TValue>> GetNonGenericCatchMethod<TException>(Func<TException, Result<TValue>> catchMethod)
            where TException : Exception
            => ex => (ex as TException)?.Map(catchMethod) ?? throw new ArgumentException("Internal error. Either the exception was not of the expected type or the catch did not return a value");
    }
}
