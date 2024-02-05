// Licensed under the MIT license.
// Copyright (c) The AppCore .NET project.

using System;

namespace AppCoreNet.Data;

/// <summary>
/// Provides a logger for data providers.
/// </summary>
/// <typeparam name="T">The category of the logger.</typeparam>
public abstract class DataProviderLogger<T>
    where T : IDataProvider
{
    /// <summary>
    /// Entity is being created.
    /// </summary>
    /// <param name="entity">The <see cref="IEntity"/>.</param>
    public abstract void EntityCreating(IEntity entity);

    /// <summary>
    /// Entity has been created.
    /// </summary>
    /// <param name="entity">The <see cref="IEntity"/>.</param>
    /// <param name="elapsedTimeMs">The elapsed time in milliseconds.</param>
    public abstract void EntityCreated(IEntity entity, long elapsedTimeMs);

    /// <summary>
    /// Entity could not be created.
    /// </summary>
    /// <param name="error">The <see cref="Exception"/> that was caught.</param>
    /// <param name="entity">The <see cref="IEntity"/>.</param>
    public abstract void EntityCreateFailed(Exception error, IEntity entity);

    /// <summary>
    /// Entity is being updated.
    /// </summary>
    /// <param name="entity">The <see cref="IEntity"/>.</param>
    public abstract void EntityUpdating(IEntity entity);

    /// <summary>
    /// Entity has been updated.
    /// </summary>
    /// <param name="entity">The <see cref="IEntity"/>.</param>
    /// <param name="elapsedTimeMs">The elapsed time in milliseconds.</param>
    public abstract void EntityUpdated(IEntity entity, long elapsedTimeMs);

    /// <summary>
    /// Entity could not be updated.
    /// </summary>
    /// <param name="error">The <see cref="Exception"/> that was caught.</param>
    /// <param name="entity">The <see cref="IEntity"/>.</param>
    public abstract void EntityUpdateFailed(Exception error, IEntity entity);

    /// <summary>
    /// Entity is being deleted.
    /// </summary>
    /// <param name="entity">The <see cref="IEntity"/>.</param>
    public abstract void EntityDeleting(IEntity entity);

    /// <summary>
    /// Entity has been deleted.
    /// </summary>
    /// <param name="entity">The <see cref="IEntity"/>.</param>
    /// <param name="elapsedTimeMs">The elapsed time in milliseconds.</param>
    public abstract void EntityDeleted(IEntity entity, long elapsedTimeMs);

    /// <summary>
    /// Entity could not be deleted.
    /// </summary>
    /// <param name="error">The <see cref="Exception"/> that was caught.</param>
    /// <param name="entity">The <see cref="IEntity"/>.</param>
    public abstract void EntityDeleteFailed(Exception error, IEntity entity);

    /// <summary>
    /// Entity is being loaded.
    /// </summary>
    /// <param name="entityType">The type of <see cref="IEntity"/>.</param>
    /// <param name="entityId">The entity id.</param>
    public abstract void EntityLoading(Type entityType, object entityId);

    /// <summary>
    /// Entity has been loaded.
    /// </summary>
    /// <param name="entity">The <see cref="IEntity"/>.</param>
    /// <param name="elapsedTimeMs">The elapsed time in milliseconds.</param>
    public abstract void EntityLoaded(IEntity entity, long elapsedTimeMs);

    /// <summary>
    /// Entity could not be loaded.
    /// </summary>
    /// <param name="error">The <see cref="Exception"/> that was caught.</param>
    /// <param name="entityType">The type of <see cref="IEntity"/>.</param>
    /// <param name="entityId">The entity id.</param>
    public abstract void EntityLoadFailed(Exception error, Type entityType, object entityId);

    /// <summary>
    /// Entity was not found.
    /// </summary>
    /// <param name="entityType">The type of <see cref="IEntity"/>.</param>
    /// <param name="entityId">The entity id.</param>
    public abstract void EntityNotFound(Type entityType, object entityId);

    /// <summary>
    /// Query is being executed.
    /// </summary>
    /// <param name="query">The <see cref="IQuery{TEntity,TResult}"/>.</param>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public abstract void QueryExecuting<TEntity, TResult>(IQuery<TEntity, TResult> query)
        where TEntity : class, IEntity;

    /// <summary>
    /// Query has been executed.
    /// </summary>
    /// <param name="query">The <see cref="IQuery{TEntity,TResult}"/>.</param>
    /// <param name="elapsedTimeMs">The elapsed time in milliseconds.</param>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public abstract void QueryExecuted<TEntity, TResult>(IQuery<TEntity, TResult> query, long elapsedTimeMs)
        where TEntity : class, IEntity;

    /// <summary>
    /// Query could not be executed.
    /// </summary>
    /// <param name="error">The <see cref="Exception"/> that occured.</param>
    /// <param name="query">The <see cref="IQuery{TEntity,TResult}"/>.</param>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TResult">The type of the result.</typeparam>
    public abstract void QueryExecuteFailed<TEntity, TResult>(Exception error, IQuery<TEntity, TResult> query)
        where TEntity : class, IEntity;

    /// <summary>
    /// Transaction has been created.
    /// </summary>
    /// <param name="transaction">The <see cref="ITransaction"/>.</param>
    public abstract void TransactionCreated(ITransaction transaction);

    /// <summary>
    /// Transaction is being committed.
    /// </summary>
    /// <param name="transaction">The <see cref="ITransaction"/>.</param>
    public abstract void TransactionCommitting(ITransaction transaction);

    /// <summary>
    /// Transaction has been committed.
    /// </summary>
    /// <param name="transaction">The <see cref="ITransaction"/>.</param>
    /// <param name="elapsedTimeMs">The elapsed time in milliseconds.</param>
    public abstract void TransactionCommitted(ITransaction transaction, long elapsedTimeMs);

    /// <summary>
    /// Transaction commit failed.
    /// </summary>
    /// <param name="error">The <see cref="Exception"/> that occured.</param>
    /// <param name="transaction">The <see cref="ITransaction"/>.</param>
    public abstract void TransactionCommitFailed(Exception error, ITransaction transaction);

    /// <summary>
    /// Transaction is being rolled back.
    /// </summary>
    /// <param name="transaction">The <see cref="ITransaction"/>.</param>
    public abstract void TransactionRollingback(ITransaction transaction);

    /// <summary>
    /// Transaction has been rolled back.
    /// </summary>
    /// <param name="transaction">The <see cref="ITransaction"/>.</param>
    public abstract void TransactionRolledback(ITransaction transaction);
}