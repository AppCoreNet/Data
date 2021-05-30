// Licensed under the MIT License.
// Copyright (c) 2020-2021 the AppCore .NET project.

using AppCore.Data.EntityFrameworkCore;
using AppCore.DependencyInjection;
using AppCore.DependencyInjection.Facilities;
using Microsoft.EntityFrameworkCore;
using ComponentRegistration = AppCore.DependencyInjection.ComponentRegistration;
using IComponentRegistry = AppCore.DependencyInjection.IComponentRegistry;

// ReSharper disable once CheckNamespace
namespace AppCore.Data
{
    /// <summary>
    /// Provides the EntityFramework Core extension for the <see cref="DataProviderFacility"/>.
    /// </summary>
    /// <typeparam name="TTag">The data provider tag.</typeparam>
    /// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
    public class EntityFrameworkCoreFacilityExtension<TTag, TDbContext> : FacilityExtension
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
                new[]
                {
                    ComponentRegistration.Scoped<IDataProvider>(
                        ComponentFactory.Create(c => c.Resolve<IDbContextDataProvider<TDbContext>>())),

                    ComponentRegistration.Scoped(
                        ComponentFactory.Create(
                            c => c.Resolve<IDbContextDataProvider<TDbContext>>()
                                  .TransactionManager))
                });
        }

        public EntityFrameworkCoreFacilityExtension<TTag, TDbContext> WithRepository<TId, TEntity, TDbEntity>()
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