namespace BluePrism.Api
{
    using System;
    using System.Collections.Generic;
    using Func;
    using Func.AspNet;

    public class ApiErrorResponseConverter : IErrorResponseConverter
    {
        private readonly IReadOnlyDictionary<Type, Func<ResultError, ResponseDetails, ErrorResponse>> _errorMappers;

        public ApiErrorResponseConverter(IReadOnlyDictionary<Type, Func<ResultError, ResponseDetails, ErrorResponse>> errorMappers)
        {
            _errorMappers = errorMappers;
        }

        public ErrorResponse GetErrorResponse<TError>(TError error, ResponseDetails configuredResponseDetails)
            where TError : ResultError
        {
            if (_errorMappers.ContainsKey(typeof(TError)))
                return _errorMappers[typeof(TError)](error, configuredResponseDetails);

            return new ErrorResponse
            {
                StatusCode = configuredResponseDetails.StatusCode,
                Body = configuredResponseDetails.Message
            };
        }
    }
}
