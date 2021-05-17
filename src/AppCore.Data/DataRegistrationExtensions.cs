// Licensed under the MIT License.
// Copyright (c) 2020-2021 the AppCore .NET project.

using System;
using AppCore.Data;

// ReSharper disable once CheckNamespace
namespace AppCore.DependencyInjection
{
    /// <summary>
    /// Provides extension methods to register the <see cref="DataFacility"/>.
    /// </summary>
    public static class DataRegistrationExtensions
    {
        /// <summary>
        /// Adds the <see cref="DataFacility"/> to the <see cref="IComponentRegistry"/>.
        /// </summary>
        /// <param name="registry">The <see cref="IComponentRegistry"/>.</param>
        /// <param name="configure">The configuration delegate.</param>
        /// <returns>The <see cref="IComponentRegistry"/>.</returns>
        public static IComponentRegistry AddData(
            this IComponentRegistry registry,
            Action<DataFacility> configure = null)
        {
            return registry.AddFacility(configure);
        }
    }
}