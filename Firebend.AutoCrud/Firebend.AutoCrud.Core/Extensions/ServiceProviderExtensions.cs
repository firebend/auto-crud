using System;
using Microsoft.Extensions.DependencyInjection;

namespace Firebend.AutoCrud.Core.Extensions
{
    public static class ServiceProviderExtensions
    {
        public static TService TryGetService<TService>(this IServiceProvider provider)
        {
            try
            {
                return provider.GetService<TService>();
            }
            catch
            {
                return default;
            }
        }
    }
}
