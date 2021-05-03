// Licensed under the MIT License.
// Copyright (c) 2020-2021 the AppCore .NET project.

using AppCore.Data;
using AppCore.DependencyInjection.Facilities;

// ReSharper disable once CheckNamespace
namespace AppCore.DependencyInjection
{
    /// <summary>
    /// Represents the data facility.
    /// </summary>
    public sealed class DataFacility : Facility
    {
        /// <inheritdoc />
        protected override void Build(IComponentRegistry registry)
        {
            base.Build(registry);

            registry.TryAdd(ComponentRegistration.Singleton<ITokenGenerator, TokenGenerator>());
            registry.TryAddEnumerable(ComponentRegistration.Scoped(typeof(IDataProvider<>), typeof(DataProvider<>)));
            registry.TryAddEnumerable(
                ComponentRegistration.Scoped(typeof(ITransactionManager<>), typeof(TransactionManager<>)));
        }
    }
}
