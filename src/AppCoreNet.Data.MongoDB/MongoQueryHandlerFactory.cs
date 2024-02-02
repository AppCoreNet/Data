// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Collections.Generic;
using System.Linq;
using AppCoreNet.Diagnostics;
using Microsoft.Extensions.DependencyInjection;

namespace AppCoreNet.Data.MongoDB;

/// <summary>
/// Provides a factory for MongoDB query handlers.
/// </summary>
public sealed class MongoQueryHandlerFactory
{
    private readonly IServiceProvider _serviceProvider;
    private readonly IEnumerable<Type> _queryHandlerTypes;

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoQueryHandlerFactory"/> class.
    /// </summary>
    /// <param name="serviceProvider">The <see cref="IServiceProvider"/>.</param>
    /// <param name="queryHandlerTypes">The types of the query handlers.</param>
    public MongoQueryHandlerFactory(IServiceProvider serviceProvider, IEnumerable<Type> queryHandlerTypes)
    {
        Ensure.Arg.NotNull(serviceProvider);
        Ensure.Arg.NotNull(queryHandlerTypes);

        _serviceProvider = serviceProvider;
        _queryHandlerTypes = queryHandlerTypes;
    }

    /// <summary>
    /// Creates a query handler for the specified query.
    /// </summary>
    /// <param name="provider">The <see cref="MongoDataProvider"/>.</param>
    /// <param name="query">The query.</param>
    /// <typeparam name="TEntity">The type of the <see cref="IEntity"/>.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    /// <returns>The <see cref="IMongoQueryHandler{TEntity,TResult}"/>.</returns>
    /// <exception cref="InvalidOperationException">There is no handler registered for the specified query.</exception>
    public IMongoQueryHandler<TEntity, TResult> CreateHandler<TEntity, TResult>(
        MongoDataProvider provider,
        IQuery<TEntity, TResult> query)
        where TEntity : class, IEntity
    {
        Ensure.Arg.NotNull(provider);
        Ensure.Arg.NotNull(query);

        Type queryHandlerType = typeof(IMongoQueryHandler<TEntity, TResult>);

        IEnumerable<Type> eligibleHandlers = _queryHandlerTypes.Where(
            t => queryHandlerType.IsAssignableFrom(t));

        foreach (Type handlerType in eligibleHandlers)
        {
            var handler =
                (IMongoQueryHandler<TEntity, TResult>)ActivatorUtilities.CreateInstance(
                    _serviceProvider,
                    handlerType,
                    provider);

            if (handler.CanExecute(query))
                return handler;
        }

        throw new InvalidOperationException(
            $"There is no handler for query type '{query.GetType().GetDisplayName()}' registered.");
    }
}