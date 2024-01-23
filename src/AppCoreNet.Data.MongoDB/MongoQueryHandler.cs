// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System.Threading;
using System.Threading.Tasks;
using AppCoreNet.Diagnostics;
using MongoDB.Driver;

namespace AppCoreNet.Data.MongoDB;

/// <summary>
/// Provides a base class for MongoDB query handlers.
/// </summary>
/// <typeparam name="TQuery">The type of the <see cref="IQuery{TEntity,TResult}"/>.</typeparam>
/// <typeparam name="TEntity">The type of the <see cref="IEntity"/>.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <typeparam name="TDbEntity">The type of the DB entity.</typeparam>
public abstract class MongoQueryHandler<TQuery, TEntity, TResult, TDbEntity> : IMongoQueryHandler<TEntity, TResult>
    where TQuery : IQuery<TEntity, TResult>
    where TEntity : class, IEntity
    where TDbEntity : class
{
    /// <summary>
    /// Gets the name of the collection which is being queried.
    /// </summary>
    protected abstract string CollectionName { get; }

    /// <summary>
    /// Gets the <see cref="MongoCollectionSettings"/> used by the query.
    /// </summary>
    protected virtual MongoCollectionSettings? CollectionSettings { get; }

    /// <summary>
    /// Gets the <see cref="MongoDataProvider"/> used by the query.
    /// </summary>
    protected MongoDataProvider Provider { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoQueryHandler{TQuery,TEntity,TResult,TDbEntity}"/> class.
    /// </summary>
    /// <param name="provider">The <see cref="MongoDataProvider"/>.</param>
    protected MongoQueryHandler(MongoDataProvider provider)
    {
        Ensure.Arg.NotNull(provider);
        Provider = provider;
    }

    /// <summary>
    /// Must be implemented to query the result.
    /// </summary>
    /// <param name="collection">The collection to query.</param>
    /// <param name="query">The <see cref="IQuery{TEntity,TResult}"/> for which results must be queried.</param>
    /// <param name="cancellationToken">Token which can be used to cancel the process.</param>
    /// <returns>The result of the query.</returns>
    protected abstract Task<TResult> QueryResultAsync(
        IMongoCollection<TDbEntity> collection,
        TQuery query,
        CancellationToken cancellationToken);

    /// <summary>
    /// Executes the given query.
    /// </summary>
    /// <param name="query">The query to execute.</param>
    /// <param name="cancellationToken">Token which can be used to cancel the process.</param>
    /// <returns>The result of the query.</returns>
    public virtual async Task<TResult> ExecuteAsync(TQuery query, CancellationToken cancellationToken = default)
    {
        Ensure.Arg.NotNull(query);

        IMongoCollection<TDbEntity> collection =
            Provider.Database.GetCollection<TDbEntity>(CollectionName, CollectionSettings);

        TResult result = await QueryResultAsync(collection, query, cancellationToken)
            .ConfigureAwait(false);

        return result;
    }

    bool IMongoQueryHandler<TEntity, TResult>.CanExecute(IQuery<TEntity, TResult> query)
    {
        return query is TQuery;
    }

    Task<TResult> IMongoQueryHandler<TEntity, TResult>.ExecuteAsync(
        IQuery<TEntity, TResult> query,
        CancellationToken cancellationToken)
    {
        Ensure.Arg.NotNull(query);
        return ExecuteAsync((TQuery)query, cancellationToken);
    }
}