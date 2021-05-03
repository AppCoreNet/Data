// Licensed under the MIT License.
// Copyright (c) 2020-2021 the AppCore .NET project.

using AppCore.Data.EntityFrameworkCore;
using AppCore.DependencyInjection;
using AppCore.DependencyInjection.Facilities;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace AppCore.Data
{
    public class EntityFrameworkCoreExtension<TTag, TDbContext> : FacilityExtension
        where TDbContext : DbContext
    {
        /// <inheritdoc />
        protected override void Build(IComponentRegistry registry)
        {
            base.Build(registry);

            registry.TryAdd(
                ComponentRegistration
                    .Scoped<IDbContextDataProvider<TDbContext>, DbContextDataProvider<TTag, TDbContext>>());

            registry.TryAddEnumerable(
                ComponentRegistration.Scoped<IDataProvider>(
                    Factory.Create(c => c.Resolve<IDbContextDataProvider<TDbContext>>())));

            registry.TryAddEnumerable(
                ComponentRegistration.Scoped<ITransactionManager>(
                    Factory.Create(
                        c => c.Resolve<IDbContextDataProvider<TDbContext>>()
                              .TransactionManager)));
        }

        /*
        public void RegisterRepository<TEntity>()
            where TEntity : class, IEntity
        {
            _registrationCallbacks.Add(
                (r, f) =>
                {
                    r.Register<IRepository<TEntity>>()
                     .Add(c => c.Resolve<IDataProvider<TTag>>().Repository<TEntity>())
                     .PerScope()
                     .IfNoneRegistered();
                });
        }
        */
    }
}