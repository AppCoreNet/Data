// Licensed under the MIT License.
// Copyright (c) 2020-2021 the AppCore .NET project.

using System;
using AppCore.Data;
using AppCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace AppCore.DependencyInjection
{
    /// <summary>
    /// Provides extension methods to register the data services.
    /// </summary>
    public static class AppCoreBuilderExtensions
    {
        /// <summary>
        /// Adds the data services to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="builder">The <see cref="IAppCoreBuilder"/>.</param>
        /// <returns>The <see cref="IDataProvidersBuilder"/>.</returns>
        public static IDataProvidersBuilder AddDataProviders(this IAppCoreBuilder builder)
        {
            Ensure.Arg.NotNull(builder);

            IServiceCollection services = builder.Services;

            services.TryAddSingleton<ITokenGenerator, TokenGenerator>();
            services.TryAddEnumerable(new []
            {
                ServiceDescriptor.Scoped(typeof(IDataProvider<>), typeof(DataProvider<>)),
                ServiceDescriptor.Scoped(typeof(ITransactionManager<>), typeof(TransactionManager<>))
            });

            return new DataProvidersBuilder(services);
        }
    }
}