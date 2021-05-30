// Licensed under the MIT License.
// Copyright (c) 2020-2021 the AppCore .NET project.

using System;
using AppCore.Data;

// ReSharper disable once CheckNamespace
namespace AppCore.DependencyInjection
{
    /// <summary>
    /// Provides extension methods to register the <see cref="DataProviderFacility"/>.
    /// </summary>
    public static class DataProviderRegistrationExtensions
    {
        /// <summary>
        /// Adds the <see cref="DataProviderFacility"/> to the <see cref="IComponentRegistry"/>.
        /// </summary>
        /// <param name="registry">The <see cref="IComponentRegistry"/>.</param>
        /// <param name="configure">The configuration delegate.</param>
        /// <returns>The <see cref="IComponentRegistry"/>.</returns>
        public static IComponentRegistry AddDataProvider(
            this IComponentRegistry registry,
            Action<DataProviderFacility> configure = null)
        {
            return registry.AddFacility(configure);
        }
    }
}