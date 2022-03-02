namespace BluePrism.Api
{
    using System;
    using System.Net;
    using Func.AspNet;
    using Logging;

    public class ApiExceptionResponseConverter : IExceptionResponseConverter
    {
        private readonly ILogger _logger;

        public ApiExceptionResponseConverter(ILogger<ApiExceptionResponseConverter> logger)
        {
            _logger = logger;
        }

        public ErrorResponse GetExceptionResponse(Exception exception)
        {
            _logger.Error(exception, "An unexpected exception occurred");

            return new ErrorResponse
            {
                StatusCode = HttpStatusCode.InternalServerError,
                Body = new { }
            };
        }
    }
}
