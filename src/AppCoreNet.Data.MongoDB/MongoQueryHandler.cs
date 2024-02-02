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
/// <typeparam name="TDocument">The type of the MongoDB document.</typeparam>
public abstract class MongoQueryHandler<TQuery, TEntity, TResult, TDocument> : IMongoQueryHandler<TEntity, TResult>
    where TQuery : IQuery<TEntity, TResult>
    where TEntity : class, IEntity
    where TDocument : class
{
    /// <summary>
    /// Gets the <see cref="IMongoCollection{TDocument}"/> used by the query handler.
    /// </summary>
    protected IMongoCollection<TDocument> Collection { get; }

    /// <summary>
    /// Gets the <see cref="MongoDataProvider"/> used by the query.
    /// </summary>
    protected MongoDataProvider Provider { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="MongoQueryHandler{TQuery,TEntity,TResult,TDocument}"/> class.
    /// </summary>
    /// <param name="provider">The <see cref="MongoDataProvider"/>.</param>
    /// <param name="collectionName">The name of the collection.</param>
    /// <param name="collectionSettings">The settings used when accessing the collection.</param>
    protected MongoQueryHandler(
        MongoDataProvider provider,
        string? collectionName = null,
        MongoCollectionSettings? collectionSettings = null)
    {
        Ensure.Arg.NotNull(provider);
        Ensure.Arg.NotEmptyButNull(collectionName);

        Provider = provider;
        Collection = provider.Database.GetCollection<TDocument>(
            string.IsNullOrEmpty(collectionName)
                ? provider.GetCollectionName<TEntity>()
                : collectionName,
            collectionSettings);
    }

    /// <summary>
    /// Must be implemented to query the result.
    /// </summary>
    /// <param name="query">The <see cref="IQuery{TEntity,TResult}"/> for which results must be queried.</param>
    /// <param name="cancellationToken">Token which can be used to cancel the process.</param>
    /// <returns>The result of the query.</returns>
    protected abstract Task<TResult> QueryResultAsync(
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

        TResult result = await QueryResultAsync(query, cancellationToken)
            .ConfigureAwait(false);

        return result;
    }

    /// <inheritdoc />
    bool IMongoQueryHandler<TEntity, TResult>.CanExecute(IQuery<TEntity, TResult> query)
    {
        return query is TQuery;
    }

    /// <inheritdoc />
    Task<TResult> IMongoQueryHandler<TEntity, TResult>.ExecuteAsync(
        IQuery<TEntity, TResult> query,
        CancellationToken cancellationToken)
    {
        Ensure.Arg.NotNull(query);
        return ExecuteAsync((TQuery)query, cancellationToken);
    }
}