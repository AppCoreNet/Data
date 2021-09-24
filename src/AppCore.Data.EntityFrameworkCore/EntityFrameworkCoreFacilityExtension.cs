// Licensed under the MIT License.
// Copyright (c) 2020-2021 the AppCore .NET project.

using System;
using System.Collections.Generic;
using AppCore.Data;
using AppCore.Data.EntityFrameworkCore;
using AppCore.DependencyInjection.Facilities;
using AppCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

// ReSharper disable once CheckNamespace
namespace AppCore.DependencyInjection
{
    /// <summary>
    /// Provides the EntityFramework Core extension for the <see cref="DataProviderFacility"/>.
    /// </summary>
    /// <typeparam name="TTag">The data provider tag.</typeparam>
    /// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
    public class EntityFrameworkCoreFacilityExtension<TTag, TDbContext> : FacilityExtension
        where TDbContext : DbContext
    {
        private int? _poolSize;
        private readonly List<Action<IServiceProvider, DbContextOptionsBuilder>> _options = new();

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

            if (_poolSize.HasValue)
            {
                services.AddDbContextPool<TDbContext>(o => { }, (int) _poolSize);

                foreach (Action<IServiceProvider, DbContextOptionsBuilder> options in _options)
                {
                    services.AddDbContextPool<TDbContext>(options, (int)_poolSize);
                }
            }
            else
            {
                services.AddDbContext<TDbContext>();

                foreach (Action<IServiceProvider, DbContextOptionsBuilder> options in _options)
                {
                    services.AddDbContext<TDbContext>(options);
                }
            }
        }

        public EntityFrameworkCoreFacilityExtension<TTag, TDbContext> WithPoolSize(int? poolSize = 128)
        {
            _poolSize = poolSize;
            return this;
        }

        public EntityFrameworkCoreFacilityExtension<TTag, TDbContext> WithOptions(Action<IServiceProvider, DbContextOptionsBuilder> options)
        {
            Ensure.Arg.NotNull(options, nameof(options));
            _options.Add(options);
            return this;
        }

        public EntityFrameworkCoreFacilityExtension<TTag, TDbContext> WithOptions(Action<DbContextOptionsBuilder> options)
        {
            return WithOptions((_, o) => options(o));
        }

        public EntityFrameworkCoreFacilityExtension<TTag, TDbContext> WithRepository<TId, TEntity, TDbEntity>()
            where TEntity : IEntity<TId>
            where TDbEntity : class
        {
            ConfigureServices(
                services =>
                    services
                        .TryAddScoped<IRepository<TId, TEntity>,
                            DbContextRepository<TId, TEntity, TDbContext, TDbEntity>>());

            return this;
        }
    }
}