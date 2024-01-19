// Licensed under the MIT License.
// Copyright (c) 2022 the AppCore .NET project.

using System;
using System.Collections.Generic;
using System.Linq;
using AppCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace AppCore.Data.EntityFrameworkCore;

/// <summary>
/// Provides query handler factory for <see cref="DbContext"/>.
/// </summary>
public sealed class DbContextQueryHandlerFactory<TDbContext>
    where TDbContext : DbContext
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<Type> _queryHandlerTypes;

    /// <summary>
    /// Initializes an instance of the <see cref="DbContextQueryHandlerFactory{TDbContext}"/> class.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
    /// <param name="queryHandlerTypes">The types of the query handlers.</param>
    public DbContextQueryHandlerFactory(IServiceProvider serviceProvider, IEnumerable<Type> queryHandlerTypes)
    {
        Ensure.Arg.NotNull(serviceProvider);
        Ensure.Arg.NotNull(queryHandlerTypes);

        _serviceProvider = serviceProvider;
        _queryHandlerTypes = queryHandlerTypes;
    }

    /// <summary>
    /// Creates a query handler for the specified query.
    /// </summary>
    /// <param name="provider">The <see cref="DbContextDataProvider{TDbContext}"/>.</param>
    /// <param name="query">The query.</param>
    /// <typeparam name="TEntity">The type of the <see cref="IEntity"/>.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <returns>The <see cref="IDbContextQueryHandler{TEntity,TResult,TDbContext}"/>.</returns>
    /// <exception cref="InvalidOperationException">There is no handler registered for the specified query.</exception>
    public IDbContextQueryHandler<TEntity, TResult, TDbContext> CreateHandler<TEntity, TResult>(
        DbContextDataProvider<TDbContext> provider,
        IQuery<TEntity, TResult> query)
        where TEntity : class, IEntity
    {
        Ensure.Arg.NotNull(provider);
        Ensure.Arg.NotNull(query);

        Type queryHandlerType = typeof(IDbContextQueryHandler<TEntity, TResult, TDbContext>);

        IEnumerable<Type> eligibleHandlers = _queryHandlerTypes.Where(
            t => queryHandlerType.IsAssignableFrom(t));

        foreach (Type handlerType in eligibleHandlers)
        {
            var handler = (IDbContextQueryHandler<TEntity, TResult, TDbContext>)
                ActivatorUtilities.CreateInstance(_serviceProvider, handlerType, provider);

            if (handler.CanExecute(query))
                return handler;
        }

        throw new InvalidOperationException(
            $"There is no handler for query type '{query.GetType().GetDisplayName()}' registered.");
    }
}