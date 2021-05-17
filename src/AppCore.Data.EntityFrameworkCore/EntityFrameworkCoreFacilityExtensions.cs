// Licensed under the MIT License.
// Copyright (c) 2020-2021 the AppCore .NET project.

using System;
using AppCore.Data;
using AppCore.Diagnostics;
using Microsoft.EntityFrameworkCore;

// ReSharper disable once CheckNamespace
namespace AppCore.DependencyInjection
{
    /// <summary>
    /// Provides extension methods to register EntityFramework Core with the <see cref="DataFacility"/>.
    /// </summary>
    public static class EntityFrameworkCoreFacilityExtensions
    {
        /// <summary>
        /// Registers an EntityFramework Core data provider with the <see cref="DataFacility"/>.
        /// </summary>
        /// <typeparam name="TTag">The data provider tag.</typeparam>
        /// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
        /// <param name="facility">The <see cref="DataFacility"/>.</param>
        /// <param name="configure">Delegate used to configure the extension.</param>
        /// <returns>The <see cref="DataFacility"/>.</returns>
        public static DataFacility UseEntityFrameworkCore<TTag, TDbContext>(
            this DataFacility facility,
            Action<EntityFrameworkCoreExtension<TTag, TDbContext>> configure = null)
            where TDbContext : DbContext
        {
            Ensure.Arg.NotNull(facility, nameof(facility));
            facility.AddExtension(configure);
            return facility;
        }

        /// <summary>
        /// Register an EntityFramework Core data provider with the <see cref="DataFacility"/> as the <see cref="DefaultDataProvider"/>.
        /// </summary>
        /// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
        /// <param name="facility">The <see cref="DataFacility"/>.</param>
        /// <param name="configure">Delegate used to configure the extension.</param>
        /// <returns>The <see cref="DataFacility"/>.</returns>
        public static DataFacility UseEntityFrameworkCore<TDbContext>(
            this DataFacility facility,
            Action<EntityFrameworkCoreExtension<DefaultDataProvider, TDbContext>> configure = null)
            where TDbContext : DbContext
        {
            return facility.UseEntityFrameworkCore<DefaultDataProvider, TDbContext>(configure);
        }
    }
}
