﻿using System;

namespace DSFramework.Extensions.DependencyInjection
{
    /// <summary>
    ///     Application's IServiceProvider.
    /// </summary>
    public static class IoC
    {
        /// <summary>
        ///     Access point of the application's IServiceProvider.
        /// </summary>
        public static IServiceProvider ApplicationServices { get; set; }
    }
}