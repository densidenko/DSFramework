using System;
using Microsoft.Extensions.DependencyInjection;

namespace DSFramework.Extensions.DependencyInjection
{
    public class LazyFactory<T> : Lazy<T> where T : class
    {
        public LazyFactory(IServiceProvider provider)
            : base(provider.GetRequiredService<T>)
        { }
    }
}