// Licensed under the MIT License.
// Copyright (c) 2020-2021 the AppCore .NET project.

using System;
using AppCore.Data;
using AppCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace AppCore.DependencyInjection
{
    public static class EntityFrameworkCoreFacilityExtensions
    {
        public static DataFacility UseEntityFrameworkCore<TTag, TDbContext>(
            this DataFacility facility,
            Action<EntityFrameworkCoreExtension<TTag, TDbContext>> configure = null)
            where TDbContext : DbContext
        {
            Ensure.Arg.NotNull(facility, nameof(facility));

            var extension = facility.AddExtension<EntityFrameworkCoreExtension<TTag, TDbContext>>();
            configure?.Invoke(extension);
            return facility;
        }

        public static DataFacility UseEntityFrameworkCore<TDbContext>(
            this DataFacility facility,
            Action<EntityFrameworkCoreExtension<DefaultDataProvider, TDbContext>> configure = null)
            where TDbContext : DbContext
        {
            return facility.UseEntityFrameworkCore<DefaultDataProvider, TDbContext>(configure);
        }
    }
}
