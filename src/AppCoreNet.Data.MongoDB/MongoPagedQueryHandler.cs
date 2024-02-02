// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace AppCoreNet.Data.MongoDB;

/// <summary>
/// Provides a base class for MongoDB query handlers which return paged result set.
/// </summary>
/// <typeparam name="TQuery">The type of the <see cref="IQuery{TEntity,TResult}"/>.</typeparam>
/// <typeparam name="TEntity">The type of the <see cref="IEntity"/>.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <typeparam name="TDocument">The type of the MongoDB document.</typeparam>
public abstract class MongoPagedQueryHandler<TQuery, TEntity, TResult, TDocument>
    : MongoQueryHandler<TQuery, TEntity, IPagedResult<TResult>, TDocument>
    where TQuery : IPagedQuery<TEntity, TResult>
    where TEntity : class, IEntity
    where TDocument : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MongoPagedQueryHandler{TQuery,TEntity,TResult,TDocument}"/> class.
    /// </summary>
    /// <param name="provider">The <see cref="MongoDataProvider"/>.</param>
    /// <param name="collectionName">The name of the collection.</param>
    /// <param name="collectionSettings">The settings used when accessing the collection.</param>
    protected MongoPagedQueryHandler(
        MongoDataProvider provider,
        string? collectionName = null,
        MongoCollectionSettings? collectionSettings = null)
        : base(provider, collectionName, collectionSettings)
    {
    }

    /// <summary>
    /// Must be implemented to apply the query.
    /// </summary>
    /// <param name="find">The find operation builder.</param>
    /// <param name="query">The <see cref="IQuery{TEntity,TResult}"/> to apply.</param>
    /// <returns>The <see cref="FilterDefinition{TDocument}"/> with applied query.</returns>
    protected abstract IFindFluent<TDocument, TDocument> ApplyQuery(IFindFluent<TDocument, TDocument> find, TQuery query);

    /// <summary>
    /// Can be overridden to project the result from <typeparamref name="TDocument"/> to <typeparamref name="TResult"/>.
    /// </summary>
    /// <param name="documents">The documents to project.</param>
    /// <returns>The projected result.</returns>
    protected virtual IReadOnlyCollection<TResult> ProjectResult(IReadOnlyCollection<TDocument> documents)
    {
        return Provider.EntityMapper.Map<List<TResult>>(documents);
    }

    /// <summary>
    /// Can be overriden to customize the paging.
    /// </summary>
    /// <param name="find">The find operation builder.</param>
    /// <param name="query">The <see cref="IQuery{TEntity,TResult}"/> to apply.</param>
    /// <returns>The projected find operation.</returns>
    protected virtual IFindFluent<TDocument, TDocument> ApplyPaging(IFindFluent<TDocument, TDocument> find, TQuery query)
    {
        if (query.Offset > 0)
            find = find.Skip((int)query.Offset);

        return find.Limit(query.Limit);
    }

    /// <inheritdoc />
    protected override async Task<IPagedResult<TResult>> QueryResultAsync(
        TQuery query,
        CancellationToken cancellationToken)
    {
        IClientSessionHandle? sessionHandle = Provider.TransactionManager.CurrentTransaction?.SessionHandle;

        IFindFluent<TDocument, TDocument> find = sessionHandle != null
            ? Collection.Find(sessionHandle, FilterDefinition<TDocument>.Empty)
            : Collection.Find(FilterDefinition<TDocument>.Empty);

        find = ApplyQuery(find, query);

        long? totalCount = null;
        if (query.TotalCount)
        {
            totalCount = await find.CountDocumentsAsync(cancellationToken)
                                   .ConfigureAwait(false);
        }

        List<TDocument> result = await ApplyPaging(find, query)
                                       .ToListAsync(cancellationToken)
                                       .ConfigureAwait(false);

        return new PagedResult<TResult>(ProjectResult(result), totalCount);
    }
}