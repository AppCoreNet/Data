// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using AppCoreNet.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using Microsoft.Extensions.Logging;

namespace AppCoreNet.Data.EntityFrameworkCore;

/// <summary>
/// Provides a <see cref="DbContext"/> based implementation of the <see cref="IRepository{TId,TEntity}"/> interface.
/// </summary>
/// <typeparam name="TId">The type of the entity id.</typeparam>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TDbContext">The type of the <see cref="DbContext"/>.</typeparam>
/// <typeparam name="TDbEntity">The type of the database entity.</typeparam>
public class DbContextRepository<TId, TEntity, TDbContext, TDbEntity> : IDbContextRepository<TDbContext>, IRepository<TId, TEntity>
    where TEntity : class, IEntity<TId>
    where TDbContext : DbContext
    where TDbEntity : class
{
    /// <summary>
    /// Provides a base class for scalar query handlers of this repository.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public abstract class ScalarQueryHandler<TQuery, TResult> : DbContextScalarQueryHandler<TQuery, TEntity, TResult?, TDbContext, TDbEntity>
        where TQuery : IQuery<TEntity, TResult?>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ScalarQueryHandler{TQuery,TResult}"/> class.
        /// </summary>
        /// <param name="provider">The <see cref="DbContextDataProvider{TDbContext}"/>.</param>
        protected ScalarQueryHandler(DbContextDataProvider<TDbContext> provider)
            : base(provider)
        {
        }
    }

    /// <summary>
    /// Provides a base class for vector query handlers of this repository.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public abstract class VectorQueryHandler<TQuery, TResult> : DbContextVectorQueryHandler<TQuery, TEntity, TResult, TDbContext, TDbEntity>
        where TQuery : IQuery<TEntity, IReadOnlyCollection<TResult>>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="VectorQueryHandler{TQuery,TResult}"/> class.
        /// </summary>
        /// <param name="provider">The <see cref="DbContextDataProvider{TDbContext}"/>.</param>
        protected VectorQueryHandler(DbContextDataProvider<TDbContext> provider)
            : base(provider)
        {
        }
    }

    /// <summary>
    /// Provides a base class for paged query handlers of this repository.
    /// </summary>
    /// <typeparam name="TQuery">The type of the query.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public abstract class PagedQueryHandler<TQuery, TResult> : DbContextPagedQueryHandler<TQuery, TEntity, TResult, TDbContext, TDbEntity>
        where TQuery : IPagedQuery<TEntity, TResult>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="PagedQueryHandler{TQuery,TResult}"/> class.
        /// </summary>
        /// <param name="provider">The <see cref="DbContextDataProvider{TDbContext}"/>.</param>
        protected PagedQueryHandler(DbContextDataProvider<TDbContext> provider)
            : base(provider)
        {
        }
    }

    private static readonly EntityModelProperties<TId, TEntity> _entityModelProperties = new ();
    private readonly DbModelProperties _modelProperties;

    /// <inheritdoc />
    public DbContextDataProvider<TDbContext> Provider { get; }

    /// <summary>
    /// Gets the <see cref="DbSet{TDbEntity}"/>.
    /// </summary>
    protected DbSet<TDbEntity> Set => Provider.DbContext.Set<TDbEntity>();

    IDataProvider IRepository<TId, TEntity>.Provider => Provider;

    /// <summary>
    /// Initializes a new instance of the <see cref="DbContextRepository{TId,TEntity,TDbContext,TDbEntity}"/> class.
    /// </summary>
    /// <param name="provider">The data provider.</param>
    /// <param name="logger">The logger.</param>
    public DbContextRepository(DbContextDataProvider<TDbContext> provider, ILogger logger)
    {
        Ensure.Arg.NotNull(provider);
        Ensure.Arg.NotNull(logger);

        TDbContext context = provider.DbContext;

        _modelProperties = DbModelProperties.Get(
            typeof(TDbContext),
            typeof(TDbEntity),
            context.Model,
            typeof(TEntity));

        Provider = provider;
    }

    /// <summary>
    /// Can be overridden to get the primary key from the specified entity id.
    /// </summary>
    /// <param name="id">The unique entity id.</param>
    /// <returns>The primary key values.</returns>
    protected virtual object?[] GetPrimaryKey(TId id)
    {
        return _entityModelProperties.GetIdValues(id);
    }

    /// <summary>
    /// Can be overridden to apply the query expression used when searching for entities by
    /// it's primary key.
    /// </summary>
    /// <param name="queryable">The queryable.</param>
    /// <param name="id">The unique entity id.</param>
    /// <returns>The queryable filtered by primary key.</returns>
    protected virtual IQueryable<TDbEntity> ApplyPrimaryKeyExpression(IQueryable<TDbEntity> queryable, TId id)
    {
        object?[] primaryKey = GetPrimaryKey(id);

        if (primaryKey.Length == 1)
        {
            string primaryKeyPropertyName = _modelProperties.PrimaryKeyPropertyNames[0];
            object? keyValue = primaryKey[0];
            return queryable.Where(e => EF.Property<TId>(e, primaryKeyPropertyName) !.Equals(keyValue));
        }

        for (int i = 0; i < primaryKey.Length; i++)
        {
            string idPropertyName = _entityModelProperties.IdPropertyNames[i];
            string primaryKeyPropertyName = _modelProperties.PrimaryKeyPropertyNames[i];

            if (!string.Equals(idPropertyName, primaryKeyPropertyName, StringComparison.OrdinalIgnoreCase))
                throw new NotSupportedException("Entity.Id property names do not match the primary key property names.");

            object? keyValue = primaryKey[i];
            queryable = queryable.Where(
                e => EF.Property<object>(e, primaryKeyPropertyName).Equals(keyValue));
        }

        return queryable;
    }

    /// <summary>
    /// Can be overridden to apply includes to the query.
    /// </summary>
    /// <param name="queryable">The <see cref="IQueryable{T}"/>.</param>
    /// <returns>The queryable.</returns>
    protected virtual IQueryable<TDbEntity> ApplyIncludes(IQueryable<TDbEntity> queryable)
    {
        return queryable;
    }

    private void UpdateChangeToken(EntityEntry<TDbEntity> entry, TEntity entity)
    {
        if (_modelProperties.HasConcurrencyToken)
        {
            PropertyEntry<TDbEntity, string?> concurrencyToken =
                entry.Property<string?>(_modelProperties.ConcurrencyTokenPropertyName!);

            if (entity is IHasChangeTokenEx expectedChangeToken)
            {
                concurrencyToken.OriginalValue = expectedChangeToken.ExpectedChangeToken;
                if (entry.State != EntityState.Deleted)
                {
                    string? changeToken = expectedChangeToken.ChangeToken;
                    if (string.IsNullOrWhiteSpace(changeToken)
                        || string.Equals(changeToken, expectedChangeToken.ExpectedChangeToken))
                    {
                        changeToken = Provider.TokenGenerator.Generate();
                    }

                    concurrencyToken.CurrentValue = changeToken;
                }
            }
            else if (entity is IHasChangeToken changeToken)
            {
                concurrencyToken.OriginalValue = changeToken.ChangeToken;
                if (entry.State != EntityState.Deleted)
                {
                    concurrencyToken.CurrentValue = Provider.TokenGenerator.Generate();
                }
            }
        }
    }

    private IQueryable<TDbEntity> GetQueryable(TId id)
    {
        return ApplyPrimaryKeyExpression(ApplyIncludes(Set), id);
    }

    /// <inheritdoc />
    public async Task<TResult> QueryAsync<TResult>(
        IQuery<TEntity, TResult> query,
        CancellationToken cancellationToken = default)
    {
        Ensure.Arg.NotNull(query);

        IDbContextQueryHandler<TEntity, TResult, TDbContext> queryHandler =
            Provider.QueryHandlerFactory.CreateHandler(Provider, query);

        Type queryType = query.GetType();
        Provider.Logger.QueryExecuting(queryType);

        var stopwatch = new Stopwatch();

        TResult result;
        try
        {
            result = await queryHandler.ExecuteAsync(query, cancellationToken)
                                       .ConfigureAwait(false);

            Provider.Logger.QueryExecuted(queryType, stopwatch.Elapsed);
        }
        catch (Exception error)
        {
            Provider.Logger.QueryFailed(error, queryType);
            throw;
        }
        finally
        {
            await DisposeQueryHandler(queryHandler)
                .ConfigureAwait(false);
        }

        [SuppressMessage(
            "IDisposableAnalyzers.Correctness",
            "IDISP007:Don\'t dispose injected",
            Justification = "Ownership is correct.")]
        [SuppressMessage(
            "ReSharper",
            "SuspiciousTypeConversion.Global",
            Justification = "Handler may implement IDisposable.")]
        async ValueTask DisposeQueryHandler(IDbContextQueryHandler<TEntity, TResult, TDbContext> handler)
        {
            switch (handler)
            {
                case IAsyncDisposable disposable:
                    await disposable.DisposeAsync()
                                    .ConfigureAwait(false);
                    break;
                case IDisposable disposable:
                    disposable.Dispose();
                    break;
            }
        }

        return result;
    }

    /// <summary>
    /// Provides the core logic to find an entity by it's unique identifier.
    /// </summary>
    /// <param name="id">The entity identifier.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns>The entity if found, otherwise <c>null</c>.</returns>
    protected virtual async Task<TDbEntity?> FindCoreAsync(TId id, CancellationToken cancellationToken)
    {
        TDbEntity? dbEntity = await GetQueryable(id)
                                    .AsNoTracking()
                                    .FirstOrDefaultAsync(cancellationToken)
                                    .ConfigureAwait(false);

        return dbEntity;
    }

    /// <inheritdoc />
    public virtual async Task<TEntity?> FindAsync(TId id, CancellationToken cancellationToken = default)
    {
        Ensure.Arg.NotNull(id);

        TDbEntity? dbEntity = await FindCoreAsync(id, cancellationToken)
            .ConfigureAwait(false);

        return dbEntity != null
            ? Provider.EntityMapper.Map<TEntity>(dbEntity)
            : default;
    }

    /// <inheritdoc />
    public virtual async Task<TEntity> LoadAsync(TId id, CancellationToken cancellationToken = default)
    {
        TEntity? entity = await FindAsync(id, cancellationToken)
            .ConfigureAwait(false);

        if (entity == null)
        {
            throw new EntityNotFoundException(typeof(TEntity), id!);
        }

        return entity;
    }

    /// <summary>
    /// Provides the core logic to create an entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="dbEntity">The database entity.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns>The created entity.</returns>
    protected virtual ValueTask<EntityEntry<TDbEntity>> CreateCoreAsync(
        TEntity entity,
        TDbEntity dbEntity,
        CancellationToken cancellationToken)
    {
        return new ValueTask<EntityEntry<TDbEntity>>(Set.Add(dbEntity));
    }

    /// <inheritdoc />
    public virtual async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Ensure.Arg.NotNull(entity);

        Provider.Logger.EntitySaving(entity);

        var dbEntity = Provider.EntityMapper.Map<TDbEntity>(entity);

        EntityEntry<TDbEntity> dbEntry = await CreateCoreAsync(entity, dbEntity, cancellationToken)
            .ConfigureAwait(false);

        dbEntity = dbEntry.Entity;
        UpdateChangeToken(dbEntry, entity);

        await Provider.SaveChangesAsync(cancellationToken)
                      .ConfigureAwait(false);

        entity = Provider.EntityMapper.Map<TEntity>(dbEntity);
        Provider.Logger.EntitySaved(entity);

        return entity;
    }

    /// <summary>
    /// Provides the core logic to update an entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="dbEntity">The database entity.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns>The updated entity.</returns>
    protected virtual ValueTask<EntityEntry<TDbEntity>> UpdateCoreAsync(
        TEntity entity,
        TDbEntity dbEntity,
        CancellationToken cancellationToken)
    {
        return new ValueTask<EntityEntry<TDbEntity>>(Set.Update(dbEntity));
    }

    /// <inheritdoc />
    public virtual async Task<TEntity> UpdateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Ensure.Arg.NotNull(entity);

        if (entity.IsTransient())
        {
            throw new InvalidOperationException(
                $"The entity cannot be updated because the '{nameof(IEntity<TId>.Id)}' property has the default value.");
        }

        Provider.Logger.EntitySaving(entity);

        TDbEntity? dbEntity = await GetQueryable(entity.Id)
                                    .FirstOrDefaultAsync(cancellationToken)
                                    .ConfigureAwait(false);

        if (dbEntity == null)
            throw new EntityConcurrencyException();

        Provider.EntityMapper.Map(entity, dbEntity);

        EntityEntry<TDbEntity> dbEntry = await UpdateCoreAsync(entity, dbEntity, cancellationToken)
            .ConfigureAwait(false);

        dbEntity = dbEntry.Entity;
        UpdateChangeToken(dbEntry, entity);

        await Provider.SaveChangesAsync(cancellationToken)
                      .ConfigureAwait(false);

        entity = Provider.EntityMapper.Map<TEntity>(dbEntity);
        Provider.Logger.EntitySaved(entity);

        return entity;
    }

    /// <summary>
    /// Provides the core logic to delete an entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="dbEntity">The database entity.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns>The deleted entity.</returns>
    protected virtual ValueTask<EntityEntry<TDbEntity>> DeleteCoreAsync(
        TEntity entity,
        TDbEntity dbEntity,
        CancellationToken cancellationToken)
    {
        return new ValueTask<EntityEntry<TDbEntity>>(Set.Remove(dbEntity));
    }

    /// <inheritdoc />
    public virtual async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Ensure.Arg.NotNull(entity);

        Provider.Logger.EntityDeleting(entity);

        var dbEntity = Provider.EntityMapper.Map<TDbEntity>(entity);

        EntityEntry<TDbEntity> dbEntry = await DeleteCoreAsync(entity, dbEntity, cancellationToken)
            .ConfigureAwait(false);

        UpdateChangeToken(dbEntry, entity);

        await Provider.SaveChangesAsync(cancellationToken)
                      .ConfigureAwait(false);

        Provider.Logger.EntityDeleted(entity);
    }
}