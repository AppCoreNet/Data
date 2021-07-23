// Licensed under the MIT License.
// Copyright (c) 2020-2021 the AppCore .NET project.

using AppCore.Data.EntityFrameworkCore;
using AppCore.DependencyInjection.Facilities;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

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
        protected override void ConfigureServices(IServiceCollection services)
        {
            base.ConfigureServices(services);

            services.TryAddScoped<IDbContextDataProvider<TDbContext>, DbContextDataProvider<TTag, TDbContext>>();

            services.TryAddEnumerable(
                new[]
                {
                    ServiceDescriptor.Scoped<IDataProvider>(
                        sp => sp.GetRequiredService<IDbContextDataProvider<TDbContext>>()),
                    ServiceDescriptor.Scoped(
                        sp => sp.GetRequiredService<IDbContextDataProvider<TDbContext>>()
                                .TransactionManager)
                });
        }
        
        public EntityFrameworkCoreFacilityExtension<TTag, TDbContext> WithRepository<TId, TEntity, TDbEntity>()
            where TEntity : IEntity<TId>
            where TDbEntity : class
        {
            AddCallback(
                services =>
                    services
                        .TryAddScoped<IRepository<TId, TEntity>,
                            DbContextRepository<TId, TEntity, TDbContext, TDbEntity>>());

            return this;
        }
    }
}