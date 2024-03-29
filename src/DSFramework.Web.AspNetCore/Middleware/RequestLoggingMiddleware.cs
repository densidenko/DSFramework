﻿using System.Threading.Tasks;
using DSFramework.Logging.Serilog;
using DSFramework.Web.AspNetCore.Http;
using Microsoft.AspNetCore.Http;
using Serilog.Context;

namespace DSFramework.Web.AspNetCore.Middleware
{
    public class RequestLoggingMiddleware
    {
        private readonly RequestDelegate _next;

        public RequestLoggingMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            using (LogContext.PushProperty(RequestProperties.IP_ADDRESS, context.GetIp()))
            {
                using (LogContext.PushProperty(RequestProperties.USER, context.GetUser()))
                {
                    await _next(context);
                }
            }
        }
    }
}