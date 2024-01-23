using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using MongoDB.Driver;

namespace AppCoreNet.Data.MongoDB;

/// <summary>
/// Provides a base class for MongoDB query handlers which return a vector.
/// </summary>
/// <typeparam name="TQuery">The type of the <see cref="IQuery{TEntity,TResult}"/>.</typeparam>
/// <typeparam name="TEntity">The type of the <see cref="IEntity"/>.</typeparam>
/// <typeparam name="TResult">The type of the result.</typeparam>
/// <typeparam name="TDbEntity">The type of the DB entity.</typeparam>
public abstract class MongoPagedQueryHandler<TQuery, TEntity, TResult, TDbEntity>
    : MongoQueryHandler<TQuery, TEntity, IPagedResult<TResult>, TDbEntity>
    where TQuery : IPagedQuery<TEntity, TResult>
    where TEntity : class, IEntity
    where TDbEntity : class
{
    /// <summary>
    /// Initializes a new instance of the <see cref="MongoPagedQueryHandler{TQuery,TEntity,TResult,TDbEntity}"/> class.
    /// </summary>
    /// <param name="provider">The <see cref="MongoDataProvider"/>.</param>
    protected MongoPagedQueryHandler(MongoDataProvider provider)
        : base(provider)
    {
    }

    /// <summary>
    /// Must be implemented to apply the query.
    /// </summary>
    /// <param name="find">The find operation builder.</param>
    /// <param name="query">The <see cref="IQuery{TEntity,TResult}"/> to apply.</param>
    /// <returns>The <see cref="FilterDefinition{TDocument}"/> with applied query.</returns>
    protected abstract IFindFluent<TDbEntity, TDbEntity> ApplyQuery(IFindFluent<TDbEntity, TDbEntity> find, TQuery query);

    /// <summary>
    /// Must be implemented to project the result from <typeparamref name="TEntity"/> to <typeparamref name="TResult"/>.
    /// </summary>
    /// <param name="find">The find operation builder.</param>
    /// <returns>The projected find operation.</returns>
    protected abstract IFindFluent<TDbEntity, TResult> ApplyProjection(IFindFluent<TDbEntity, TDbEntity> find);

    /// <summary>
    /// Can be overriden to customize the paging.
    /// </summary>
    /// <param name="find">The find operation builder.</param>
    /// <param name="query">The <see cref="IQuery{TEntity,TResult}"/> to apply.</param>
    /// <returns>The projected find operation.</returns>
    protected virtual IFindFluent<TDbEntity, TDbEntity> ApplyPaging(IFindFluent<TDbEntity, TDbEntity> find, TQuery query)
    {
        if (query.Offset > 0)
            find = find.Skip((int)query.Offset);

        return find.Limit(query.Limit);
    }

    /// <inheritdoc />
    protected override async Task<IPagedResult<TResult>> QueryResultAsync(
        IMongoCollection<TDbEntity> collection,
        TQuery query,
        CancellationToken cancellationToken)
    {
        IClientSessionHandle? sessionHandle = Provider.TransactionManager.CurrentTransaction?.SessionHandle;

        IFindFluent<TDbEntity, TDbEntity> find = sessionHandle != null
            ? collection.Find(sessionHandle, FilterDefinition<TDbEntity>.Empty)
            : collection.Find(FilterDefinition<TDbEntity>.Empty);

        find = ApplyQuery(find, query);

        long? totalCount = null;
        if (query.TotalCount)
        {
            totalCount = await find.CountDocumentsAsync(cancellationToken)
                                   .ConfigureAwait(false);
        }

        List<TResult> result = await ApplyProjection(ApplyPaging(find, query))
                                     .ToListAsync(cancellationToken)
                                     .ConfigureAwait(false);

        return new PagedResult<TResult>(result, totalCount);
    }
}