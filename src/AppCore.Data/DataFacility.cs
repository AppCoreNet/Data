// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

using AppCore.Data;
using AppCore.DependencyInjection.Facilities;

// ReSharper disable once CheckNamespace
namespace AppCore.DependencyInjection
{
    /// <summary>
    /// Represents the data facility.
    /// </summary>
    public sealed class DataFacility : Facility, IDataFacility
    {
        /// <inheritdoc />
        protected override void RegisterComponents(IComponentRegistry registry)
        {
            registry.Register<ITokenGenerator>()
                    .Add<TokenGenerator>()
                    .PerContainer()
                    .IfNoneRegistered();

            registry.Register(typeof(IDataProvider<>))
                    .Add(typeof(DataProvider<>))
                    .PerScope()
                    .IfNotRegistered();

            registry.Register(typeof(ITransactionManager<>))
                    .Add(typeof(TransactionManager<>))
                    .PerScope()
                    .IfNotRegistered();
        }
    }
}
