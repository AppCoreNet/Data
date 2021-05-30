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
    /// Provides extension methods to register EntityFramework Core with the <see cref="DataProviderFacility"/>.
    /// </summary>
    public static class DataProviderFacilityExtensions
    {
        /// <summary>
        /// Registers an EntityFramework Core data provider with the <see cref="DataProviderFacility"/>.
        /// </summary>
        /// <typeparam name="TTag">The data provider tag.</typeparam>
        /// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
        /// <param name="facility">The <see cref="DataProviderFacility"/>.</param>
        /// <param name="configure">Delegate used to configure the extension.</param>
        /// <returns>The <see cref="DataProviderFacility"/>.</returns>
        public static DataProviderFacility UseEntityFrameworkCore<TTag, TDbContext>(
            this DataProviderFacility facility,
            Action<EntityFrameworkCoreFacilityExtension<TTag, TDbContext>> configure = null)
            where TDbContext : DbContext
        {
            Ensure.Arg.NotNull(facility, nameof(facility));
            facility.AddExtension(configure);
            return facility;
        }

        /// <summary>
        /// Register an EntityFramework Core data provider with the <see cref="DataProviderFacility"/> as the <see cref="DefaultDataProvider"/>.
        /// </summary>
        /// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
        /// <param name="facility">The <see cref="DataProviderFacility"/>.</param>
        /// <param name="configure">Delegate used to configure the extension.</param>
        /// <returns>The <see cref="DataProviderFacility"/>.</returns>
        public static DataProviderFacility UseEntityFrameworkCore<TDbContext>(
            this DataProviderFacility facility,
            Action<EntityFrameworkCoreFacilityExtension<DefaultDataProvider, TDbContext>> configure = null)
            where TDbContext : DbContext
        {
            return facility.UseEntityFrameworkCore<DefaultDataProvider, TDbContext>(configure);
        }
    }
}
