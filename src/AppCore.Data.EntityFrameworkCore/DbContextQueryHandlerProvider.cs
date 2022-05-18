// Licensed under the MIT License.
// Copyright (c) 2022 the AppCore .NET project.

using System;
using System.Collections.Generic;
using System.Linq;
using AppCore.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace AppCore.Data.EntityFrameworkCore
{
    /// <summary>
    /// Provides the default implementation of <see cref="IDbContextQueryHandlerProvider"/>.
    /// </summary>
    public class DbContextQueryHandlerProvider : IDbContextQueryHandlerProvider
    {
        private readonly IServiceProvider _serviceProvider;

        /// <summary>
        /// Initializes an instance of the <see cref="DbContextQueryHandlerProvider"/> class.
        /// </summary>
        /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
        public DbContextQueryHandlerProvider(IServiceProvider serviceProvider)
        {
            Ensure.Arg.NotNull(serviceProvider, nameof(serviceProvider));
            _serviceProvider = serviceProvider;
        }

        /// <inheritdoc />
        public IDbContextQueryHandler<TEntity, TResult> GetHandler<TEntity, TResult>(Type queryType)
            where TEntity : IEntity
        {
            Ensure.Arg.NotNull(queryType, nameof(queryType));

            IEnumerable<IDbContextQueryHandler<TEntity, TResult>> handlers =
                _serviceProvider.GetServices<IDbContextQueryHandler<TEntity, TResult>>();

            IDbContextQueryHandler<TEntity, TResult> handler =
                handlers.FirstOrDefault(h => queryType.IsAssignableFrom(h.QueryType));

            if (handler == null)
                throw new InvalidOperationException(
                    $"There is no query handler for type {queryType.GetDisplayName()} registered.");

            return handler;
        }
    }
}