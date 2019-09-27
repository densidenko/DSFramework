using System;
using System.Net;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Logging;
using Newtonsoft.Json;
using Newtonsoft.Json.Serialization;

namespace DSFramework.AspNetCore.Middleware
{
    public class ExceptionHandlerMiddleware
    {
        private readonly RequestDelegate _next;
        private readonly ILogger<ExceptionHandlerMiddleware> _logger;

        public ExceptionHandlerMiddleware(RequestDelegate next, ILogger<ExceptionHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task Invoke(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (ArgumentException e)
            {
                if (context.Response.HasStarted)
                {
                    _logger.LogWarning("The response has already started, the api exception middleware will not be executed");
                    throw;
                }

                _logger.LogInformation(e, "Exception was caught by middleware");
                context.Response.StatusCode = (int)HttpStatusCode.BadRequest;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(SerializeException(e));
            }
            catch (Exception e)
            {
                if (context.Response.HasStarted)
                {
                    _logger.LogWarning("The response has already started, the api exception middleware will not be executed");
                    throw;
                }

                _logger.LogError(e, "Exception was caught by middleware");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.ContentType = "application/json";
                await context.Response.WriteAsync(SerializeException(e));
            }
        }

        private static string SerializeException(Exception e)
        {
            return JsonConvert.SerializeObject(e.Message,
                                               Formatting.Indented,
                                               new JsonSerializerSettings
                                               {
                                                   ContractResolver = new CamelCasePropertyNamesContractResolver()
                                               });
        }
    }
}