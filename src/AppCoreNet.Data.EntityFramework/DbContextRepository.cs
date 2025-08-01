// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Data.Entity.Infrastructure;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;
using AppCoreNet.Diagnostics;

namespace AppCoreNet.Data.EntityFramework;

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

    private static readonly EntityModelProperties<TId, TEntity> _entityModelProperties = new();
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
    public DbContextRepository(DbContextDataProvider<TDbContext> provider)
    {
        Ensure.Arg.NotNull(provider);
        Provider = provider;
        _modelProperties = DbModelProperties.Get(provider.DbContext, typeof(TDbEntity), typeof(TEntity));
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
        object?[] primaryKeyValues = GetPrimaryKey(id);
        ParameterExpression parameter = Expression.Parameter(typeof(TDbEntity), "e");

        Expression? predicate = null;
        for (int i = 0; i < _modelProperties.PrimaryKeyPropertyNames.Count; i++)
        {
            string pkPropertyName = _modelProperties.PrimaryKeyPropertyNames[i];
            object? pkValue = primaryKeyValues[i];

            Expression property = Expression.Property(parameter, pkPropertyName);
            Expression value = Expression.Constant(pkValue);
            Expression equals = Expression.Equal(property, Expression.Convert(value, property.Type));

            predicate = predicate == null ? equals : Expression.AndAlso(predicate, equals);
        }

        if (predicate == null)
            return queryable;

        return queryable.Where(Expression.Lambda<Func<TDbEntity, bool>>(predicate, parameter));
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

    private void UpdateChangeToken(DbEntityEntry<TDbEntity> entry, TEntity entity)
    {
        if (_modelProperties.HasConcurrencyToken)
        {
            DbPropertyEntry<TDbEntity, string?> concurrencyTokenProperty =
                entry.Property<string?>(_modelProperties.ConcurrencyTokenPropertyName!);

            if (entity is IHasChangeTokenEx expectedChangeToken)
            {
                if (entry.State != EntityState.Added)
                {
                    concurrencyTokenProperty.OriginalValue = expectedChangeToken.ExpectedChangeToken;
                }

                if (entry.State != EntityState.Deleted)
                {
                    string? changeToken = expectedChangeToken.ChangeToken;
                    if (string.IsNullOrWhiteSpace(changeToken)
                        || string.Equals(changeToken, expectedChangeToken.ExpectedChangeToken))
                    {
                        changeToken = Provider.TokenGenerator.Generate();
                    }

                    concurrencyTokenProperty.CurrentValue = changeToken;
                }
            }
            else if (entity is IHasChangeToken changeToken)
            {
                if (entry.State != EntityState.Added)
                {
                    concurrencyTokenProperty.OriginalValue = changeToken.ChangeToken;
                }

                if (entry.State != EntityState.Deleted)
                {
                    concurrencyTokenProperty.CurrentValue = Provider.TokenGenerator.Generate();
                }
            }
        }
    }

    private IQueryable<TDbEntity> GetQueryable(TId id)
    {
        return ApplyPrimaryKeyExpression(ApplyIncludes(Set), id);
    }

    private async Task<T> DoAsync<T>(Action before, Func<Task<T>> action, Action<T, long> after, Action<Exception> failed)
    {
        before();

        var stopwatch = new Stopwatch();
        stopwatch.Start();

        try
        {
            T result = await action()
                .ConfigureAwait(false);

            after(result, stopwatch.ElapsedMilliseconds);

            return result;
        }
        catch (Exception error)
        {
            failed(error);
            throw;
        }
    }

    private async Task DoAsync(Action before, Func<Task> action, Action<long> after, Action<Exception> failed)
    {
        await DoAsync(
            before,
            async () =>
            {
                await action()
                    .ConfigureAwait(false);

                return true;
            },
            (_, elapsedTimeMs) => after(elapsedTimeMs),
            failed);
    }

    /// <inheritdoc />
    public async Task<TResult> QueryAsync<TResult>(
        IQuery<TEntity, TResult> query,
        CancellationToken cancellationToken = default)
    {
        Ensure.Arg.NotNull(query);

        IDbContextQueryHandler<TEntity, TResult, TDbContext> queryHandler =
            Provider.QueryHandlerFactory.CreateHandler(Provider, query);

        TResult result;
        try
        {
            result =
                await DoAsync(
                        () =>
                        {
                            Provider.Logger.QueryExecuting(query);
                        },
                        async () =>
                        {
                            return await queryHandler.ExecuteAsync(query, cancellationToken)
                                                     .ConfigureAwait(false);
                        },
                        (_, elapsedTimeMs) =>
                        {
                            Provider.Logger.QueryExecuted(query, elapsedTimeMs);
                        },
                        error =>
                        {
                            Provider.Logger.QueryExecuteFailed(error, query);
                        })
                    .ConfigureAwait(false);
        }
        finally
        {
            await DisposeQueryHandlerAsync(queryHandler)
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
        async ValueTask DisposeQueryHandlerAsync(IDbContextQueryHandler<TEntity, TResult, TDbContext> handler)
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

        TEntity? entity =
            await DoAsync(
                    () =>
                    {
                        Provider.Logger.EntityLoading(typeof(TEntity), id);
                    },
                    async () =>
                    {
                        TDbEntity? dbEntity = await FindCoreAsync(id, cancellationToken)
                            .ConfigureAwait(false);

                        return dbEntity != null
                            ? Provider.EntityMapper.Map<TEntity>(dbEntity)
                            : default;
                    },
                    (result, elapsedTimeMs) =>
                    {
                        if (result != default)
                        {
                            Provider.Logger.EntityLoaded(result, elapsedTimeMs);
                        }
                        else
                        {
                            Provider.Logger.EntityNotFound(typeof(TEntity), id);
                        }
                    },
                    error =>
                    {
                        Provider.Logger.EntityLoadFailed(error, typeof(TEntity), id);
                    })
                .ConfigureAwait(false);

        return entity;
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
    protected virtual ValueTask<DbEntityEntry<TDbEntity>> CreateCoreAsync(
        TEntity entity,
        TDbEntity dbEntity,
        CancellationToken cancellationToken)
    {
        TDbEntity addedDbEntity = Set.Add(dbEntity);
        return new ValueTask<DbEntityEntry<TDbEntity>>(Provider.DbContext.Entry(addedDbEntity));
    }

    /// <inheritdoc />
    public virtual async Task<TEntity> CreateAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Ensure.Arg.NotNull(entity);

        TEntity result =
            await DoAsync(
                    () =>
                    {
                        Provider.Logger.EntityCreating(entity);
                    },
                    async () =>
                    {
                        var dbEntity = Provider.EntityMapper.Map<TDbEntity>(entity);

                        DbEntityEntry<TDbEntity> dbEntry = await CreateCoreAsync(entity, dbEntity, cancellationToken)
                            .ConfigureAwait(false);

                        dbEntity = dbEntry.Entity;
                        UpdateChangeToken(dbEntry, entity);

                        await Provider.SaveChangesAsync(cancellationToken)
                                      .ConfigureAwait(false);

                        return Provider.EntityMapper.Map<TEntity>(dbEntity);
                    },
                    (e, elapsedTimeMs) =>
                    {
                        Provider.Logger.EntityCreated(e, elapsedTimeMs);
                    },
                    error =>
                    {
                        Provider.Logger.EntityCreateFailed(error, entity);
                    })
                .ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Provides the core logic to update an entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="dbEntity">The database entity.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns>The updated entity.</returns>
    protected virtual ValueTask<DbEntityEntry<TDbEntity>> UpdateCoreAsync(
        TEntity entity,
        TDbEntity dbEntity,
        CancellationToken cancellationToken)
    {
        // This method is called from UpdateAsync where dbEntity is loaded from the context
        // and then mapped. So, it should be tracked.
        // If it were detached for some reason, attaching and setting state is correct.
        DbEntityEntry<TDbEntity> entry = Provider.DbContext.Entry(dbEntity);
        if (entry.State == EntityState.Detached)
        {
            Set.Attach(dbEntity);
            entry = Provider.DbContext.Entry(dbEntity);
        }

        entry.State = EntityState.Modified;
        return new ValueTask<DbEntityEntry<TDbEntity>>(entry);
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

        TEntity result =
            await DoAsync(
                    () =>
                    {
                        Provider.Logger.EntityUpdating(entity);
                    },
                    async () =>
                    {
                        TDbEntity? dbEntity = await GetQueryable(entity.Id)
                                                    .FirstOrDefaultAsync(cancellationToken)
                                                    .ConfigureAwait(false);

                        if (dbEntity == null)
                            throw new EntityConcurrencyException();

                        Provider.EntityMapper.Map(entity, dbEntity);
                        Provider.DbContext.Entry(dbEntity).State = EntityState.Modified;

                        DbEntityEntry<TDbEntity> dbEntry = await UpdateCoreAsync(
                                entity,
                                dbEntity,
                                cancellationToken)
                            .ConfigureAwait(false);

                        dbEntity = dbEntry.Entity;
                        UpdateChangeToken(dbEntry, entity);

                        await Provider.SaveChangesAsync(cancellationToken)
                                      .ConfigureAwait(false);

                        return Provider.EntityMapper.Map<TEntity>(dbEntity);
                    },
                    (e, elapsedTimeMs) =>
                    {
                        Provider.Logger.EntityUpdated(e, elapsedTimeMs);
                    },
                    error =>
                    {
                        Provider.Logger.EntityUpdateFailed(error, entity);
                    })
                .ConfigureAwait(false);

        return result;
    }

    /// <summary>
    /// Provides the core logic to delete an entity.
    /// </summary>
    /// <param name="entity">The entity.</param>
    /// <param name="dbEntity">The database entity.</param>
    /// <param name="cancellationToken">Optional <see cref="CancellationToken"/>.</param>
    /// <returns>The deleted entity.</returns>
    protected virtual ValueTask<DbEntityEntry<TDbEntity>> DeleteCoreAsync(
        TEntity entity,
        TDbEntity dbEntity,
        CancellationToken cancellationToken)
    {
        DbEntityEntry<TDbEntity> entry = Provider.DbContext.Entry(dbEntity);
        if (entry.State == EntityState.Detached)
        {
            Set.Attach(dbEntity);
            entry = Provider.DbContext.Entry(dbEntity);
        }

        TDbEntity removedEntity = Set.Remove(dbEntity);
        return new ValueTask<DbEntityEntry<TDbEntity>>(Provider.DbContext.Entry(removedEntity));
    }

    /// <inheritdoc />
    public virtual async Task DeleteAsync(TEntity entity, CancellationToken cancellationToken = default)
    {
        Ensure.Arg.NotNull(entity);

        await DoAsync(
                () =>
                {
                    Provider.Logger.EntityDeleting(entity);
                },
                async () =>
                {
                    var dbEntity = Provider.EntityMapper.Map<TDbEntity>(entity);

                    DbEntityEntry<TDbEntity> dbEntry = await DeleteCoreAsync(entity, dbEntity, cancellationToken)
                        .ConfigureAwait(false);

                    UpdateChangeToken(dbEntry, entity);

                    await Provider.SaveChangesAsync(cancellationToken)
                                  .ConfigureAwait(false);
                },
                elapsedTimeMs =>
                {
                    Provider.Logger.EntityDeleted(entity, elapsedTimeMs);
                },
                error =>
                {
                    Provider.Logger.EntityDeleteFailed(error, entity);
                })
            .ConfigureAwait(false);
    }
}