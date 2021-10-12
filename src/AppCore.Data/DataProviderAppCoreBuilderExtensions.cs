// Licensed under the MIT License.
// Copyright (c) 2020-2021 the AppCore .NET project.

using System;
using AppCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

// ReSharper disable once CheckNamespace
namespace AppCore.DependencyInjection
{
    /// <summary>
    /// Provides extension methods to register the <see cref="DataProviderFacility"/>.
    /// </summary>
    public static class DataProviderAppCoreBuilderExtensions
    {
        /// <summary>
        /// Adds the <see cref="DataProviderFacility"/> to the <see cref="IAppCoreBuilder"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IAppCoreBuilder"/>.</param>
        /// <param name="configure">The configuration delegate.</param>
        /// <returns>The <see cref="IAppCoreBuilder"/>.</returns>
        public static IAppCoreBuilder AddDataProvider(
            this IAppCoreBuilder builder,
            Action<DataProviderFacility> configure = null)
        {
            Ensure.Arg.NotNull(builder, nameof(builder));
            builder.Services.AddFacility(configure);
            return builder;
        }
    }
}