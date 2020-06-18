// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

using System;
using AppCore.DependencyInjection.Facilities;
using Microsoft.EntityFrameworkCore;

namespace AppCore.Data.EntityFrameworkCore
{
    internal class EntityFrameworkCoreExtensionBuilder<TTag, TDbContext>
        : IEntityFrameworkCoreExtensionBuilder<TTag, TDbContext>
        where TDbContext : DbContext
    {
        private readonly IFacilityExtensionBuilder<IDataFacility, EntityFrameworkCoreExtension<TTag, TDbContext>> _builder;

        public EntityFrameworkCoreExtensionBuilder(
            IFacilityExtensionBuilder<IDataFacility, EntityFrameworkCoreExtension<TTag, TDbContext>> builder)
        {
            _builder = builder;
        }

        public void Configure(Action<IDataFacility, EntityFrameworkCoreExtension<TTag, TDbContext>> configure)
        {
            _builder.Configure(configure);
        }

        /*
        public IEntityFrameworkCoreExtensionBuilder<TTag, TDbContext> AddRepository<TEntity>()
            where TEntity : class, IEntity
        {
            _builder.Configure((f,e) => e.RegisterRepository<TEntity>());
            return this;
        }
        */
    }
}