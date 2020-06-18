// Licensed under the MIT License.
// Copyright (c) 2020 the AppCore .NET project.

using System;
using AppCore.Data;
using AppCore.Data.EntityFrameworkCore;
using AppCore.DependencyInjection.Facilities;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace AppCore.DependencyInjection
{
    public static class EntityFrameworkCoreRegistrationExtensions
    {
        public static IFacilityBuilder<IDataFacility>
            AddEntityFrameworkCore<TTag, TDbContext>(
                this IFacilityBuilder<IDataFacility> builder,
                Action<IEntityFrameworkCoreExtensionBuilder<TTag, TDbContext>> configure = null)
            where TDbContext : DbContext
        {
            return builder.Add<EntityFrameworkCoreExtension<TTag, TDbContext>>(
                c => { configure?.Invoke(new EntityFrameworkCoreExtensionBuilder<TTag, TDbContext>(c)); });
        }

        public static IFacilityBuilder<IDataFacility>
            AddEntityFrameworkCore<TDbContext>(
                this IFacilityBuilder<IDataFacility> builder,
                Action<IEntityFrameworkCoreExtensionBuilder<DefaultDataProvider, TDbContext>> configure = null)
            where TDbContext : DbContext
        {
            return AddEntityFrameworkCore<DefaultDataProvider, TDbContext>(builder, configure);
        }
    }
}
