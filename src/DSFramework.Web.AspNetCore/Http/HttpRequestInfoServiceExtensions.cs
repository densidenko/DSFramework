﻿using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace DSFramework.Web.AspNetCore.Http
{
    /// <summary>
    ///     Http Request Info Extensions
    /// </summary>
    public static class HttpRequestInfoServiceExtensions
    {
        /// <summary>
        ///     Adds IHttpContextAccessor, IActionContextAccessor, IUrlHelper and IHttpRequestInfoService to IServiceCollection.
        /// </summary>
        public static IServiceCollection AddHttpRequestInfoService(this IServiceCollection services)
        {
            services.AddHttpContextAccessor();
            services.TryAddSingleton<IActionContextAccessor, ActionContextAccessor>();
            // Allows injecting IUrlHelper as a dependency
            services.AddScoped(serviceProvider =>
            {
                var actionContext = serviceProvider.GetService<IActionContextAccessor>().ActionContext;
                var urlHelperFactory = serviceProvider.GetService<IUrlHelperFactory>();
                return urlHelperFactory?.GetUrlHelper(actionContext);
            });
            services.AddScoped<IHttpRequestInfoService, HttpRequestInfoService>();
            return services;
        }
    }

    /// <summary>
    ///     HttpRequest Info
    /// </summary>
    public interface IHttpRequestInfoService
    {
        /// <summary>
        ///     Gets the current HttpContext.Request content's absolute path.
        ///     If the specified content path does not start with the tilde (~) character, this method returns contentPath
        ///     unchanged.
        /// </summary>
        Uri AbsoluteContent(string contentPath);

        /// <summary>
        ///     Deserialize `request.Body` as a JSON content.
        /// </summary>
        Task<T> DeserializeRequestJsonBodyAsAsync<T>();

        /// <summary>
        ///     Deserialize `request.Body` as a JSON content.
        /// </summary>
        Task<Dictionary<string, string>> DeserializeRequestJsonBodyAsDictionaryAsync();

        /// <summary>
        ///     Gets the current HttpContext.Request's root address.
        /// </summary>
        Uri GetBaseUri();

        /// <summary>
        ///     Gets the current HttpContext.Request's root address.
        /// </summary>
        string GetBaseUrl();

        /// <summary>
        ///     Gets a current HttpContext.Request's header value.
        /// </summary>
        string GetHeaderValue(string headerName);

        /// <summary>
        ///     Gets the current HttpContext.Request's IP.
        /// </summary>
        string GetIP(bool tryUseXForwardHeader = true);

        /// <summary>
        ///     Gets the current HttpContext.Request's address.
        /// </summary>
        Uri GetRawUri();

        /// <summary>
        ///     Gets the current HttpContext.Request's address.
        /// </summary>
        string GetRawUrl();

        /// <summary>
        ///     Gets the current HttpContext.Request's Referrer.
        /// </summary>
        Uri GetReferrerUri();

        /// <summary>
        ///     Gets the current HttpContext.Request's Referrer.
        /// </summary>
        string GetReferrerUrl();

        /// <summary>
        ///     Gets the current HttpContext.Request's IUrlHelper.
        /// </summary>
        IUrlHelper GetUrlHelper();

        /// <summary>
        ///     Gets the current HttpContext.Request's UserAgent.
        /// </summary>
        string GetUserAgent();

        /// <summary>
        ///     Reads `request.Body` as string.
        /// </summary>
        Task<string> ReadRequestBodyAsStringAsync();
    }

    /// <summary>
    ///     Http Request Info
    /// </summary>
    public class HttpRequestInfoService : IHttpRequestInfoService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly IUrlHelper _urlHelper;

        /// <summary>
        ///     Http Request Info
        /// </summary>
        public HttpRequestInfoService(IHttpContextAccessor httpContextAccessor, IUrlHelper urlHelper)
        {
            _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
            _urlHelper = urlHelper;
        }

        /// <summary>
        ///     Gets the current HttpContext.Request's UserAgent.
        /// </summary>
        public string GetUserAgent() => GetHeaderValue("User-Agent");

        /// <summary>
        ///     Gets the current HttpContext.Request's Referrer.
        /// </summary>
        public string GetReferrerUrl() => _httpContextAccessor.HttpContext.GetReferrerUrl();

        /// <summary>
        ///     Gets the current HttpContext.Request's Referrer.
        /// </summary>
        public Uri GetReferrerUri() => _httpContextAccessor.HttpContext.GetReferrerUri();

        /// <summary>
        ///     Gets the current HttpContext.Request's IP.
        /// </summary>
        public string GetIP(bool tryUseXForwardHeader = true) => _httpContextAccessor.HttpContext.GetIp(tryUseXForwardHeader);

        /// <summary>
        ///     Gets a current HttpContext.Request's header value.
        /// </summary>
        public string GetHeaderValue(string headerName) => _httpContextAccessor.HttpContext.GetHeaderValue(headerName);

        /// <summary>
        ///     Gets the current HttpContext.Request content's absolute path.
        ///     If the specified content path does not start with the tilde (~) character, this method returns contentPath
        ///     unchanged.
        /// </summary>
        public Uri AbsoluteContent(string contentPath)
        {
            var urlHelper = _urlHelper ?? throw new NullReferenceException(nameof(_urlHelper));
            return new Uri(GetBaseUri(), urlHelper.Content(contentPath));
        }

        /// <summary>
        ///     Gets the current HttpContext.Request's root address.
        /// </summary>
        public Uri GetBaseUri() => new Uri(GetBaseUrl());

        /// <summary>
        ///     Gets the current HttpContext.Request's root address.
        /// </summary>
        public string GetBaseUrl() => _httpContextAccessor.HttpContext.GetBaseUrl();

        /// <summary>
        ///     Gets the current HttpContext.Request's address.
        /// </summary>
        public string GetRawUrl() => _httpContextAccessor.HttpContext.GetRawUrl();

        /// <summary>
        ///     Gets the current HttpContext.Request's address.
        /// </summary>
        public Uri GetRawUri() => new Uri(GetRawUrl());

        /// <summary>
        ///     Gets the current HttpContext.Request's IUrlHelper.
        /// </summary>
        public IUrlHelper GetUrlHelper() => _urlHelper ?? throw new NullReferenceException(nameof(_urlHelper));

        /// <summary>
        ///     Deserialize `request.Body` as a JSON content.
        /// </summary>
        public Task<T> DeserializeRequestJsonBodyAsAsync<T>() => _httpContextAccessor.HttpContext.DeserializeRequestJsonBodyAsAsync<T>();

        /// <summary>
        ///     Reads `request.Body` as string.
        /// </summary>
        public Task<string> ReadRequestBodyAsStringAsync() => _httpContextAccessor.HttpContext.ReadRequestBodyAsStringAsync();

        /// <summary>
        ///     Deserialize `request.Body` as a JSON content.
        /// </summary>
        public Task<Dictionary<string, string>> DeserializeRequestJsonBodyAsDictionaryAsync()
            => _httpContextAccessor.HttpContext.DeserializeRequestJsonBodyAsDictionaryAsync();
    }
}