// Licensed under the MIT License.
// Copyright (c) 2020-2021 the AppCore .NET project.

using AppCore.Data.EntityFrameworkCore;
using AppCore.DependencyInjection;
using AppCore.DependencyInjection.Facilities;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace AppCore.Data
{
    /// <summary>
    /// Provides the EntityFramework Core extension for the <see cref="DataFacility"/>.
    /// </summary>
    /// <typeparam name="TTag">The data provider tag.</typeparam>
    /// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
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
                    ComponentFactory.Create(c => c.Resolve<IDbContextDataProvider<TDbContext>>())));

            registry.TryAddEnumerable(
                ComponentRegistration.Scoped(
                    ComponentFactory.Create(
                        c => c.Resolve<IDbContextDataProvider<TDbContext>>()
                              .TransactionManager)));
        }

        public EntityFrameworkCoreExtension<TTag, TDbContext> WithRepository<TId, TEntity, TDbEntity>()
            where TEntity : IEntity<TId>
            where TDbEntity : class
        {
            ConfigureRegistry(
                r => r.TryAdd(
                    ComponentRegistration.Scoped<IRepository<TId, TEntity>, DbContextRepository<TId, TEntity, TDbContext, TDbEntity>>()));

            return this;
        }
    }
}