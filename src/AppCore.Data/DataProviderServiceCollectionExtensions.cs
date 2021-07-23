// Licensed under the MIT License.
// Copyright (c) 2020-2021 the AppCore .NET project.

using System;
using AppCore.Data;

// ReSharper disable once CheckNamespace
namespace Microsoft.Extensions.DependencyInjection
{
    /// <summary>
    /// Provides extension methods to register the <see cref="DataProviderFacility"/>.
    /// </summary>
    public static class DataProviderServiceCollectionExtensions
    {
        /// <summary>
        /// Adds the <see cref="DataProviderFacility"/> to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The <see cref="IServiceCollection"/>.</param>
        /// <param name="configure">The configuration delegate.</param>
        /// <returns>The <see cref="IServiceCollection"/>.</returns>
        public static IServiceCollection AddDataProvider(
            this IServiceCollection services,
            Action<DataProviderFacility> configure = null)
        {
            return services.AddFacility(configure);
        }
    }
}