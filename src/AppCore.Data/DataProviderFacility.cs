// Licensed under the MIT License.
// Copyright (c) 2020-2021 the AppCore .NET project.

using AppCore.DependencyInjection.Facilities;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace AppCore.Data
{
    /// <summary>
    /// Represents the data facility.
    /// </summary>
    public sealed class DataProviderFacility : Facility
    {
        /// <inheritdoc />
        protected override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);

            services.TryAddSingleton<ITokenGenerator, TokenGenerator>();
            services.TryAddEnumerable(new []
            {
                ServiceDescriptor.Scoped(typeof(IDataProvider<>), typeof(DataProvider<>)),
                ServiceDescriptor.Scoped(typeof(ITransactionManager<>), typeof(TransactionManager<>))
            });
        }
    }
}
